<?php

namespace App\Http\Controllers\Member;

use App\Enums\AgentCustomizedSettingsEnum;
use App\Enums\WalletTypeEnum;
use App\Enums\TransactionTypeEnum;
use Carbon\Carbon;
use Illuminate\Http\JsonResponse;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\Hash;
use Illuminate\Support\Facades\DB;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Helpers\StringHelper;
use App\Http\Controllers\Controller;
use App\Helpers\Common;
use App\Services\AgentDomainService;
use App\Services\AgentService;
use App\Services\AvatarService;
use App\Services\MemberService;
use App\Services\SmsService;
use App\Services\MemberMinerService;
use App\Services\TransactionLogService;
use App\Models\Member;
use App\Models\MemberAccessToken;
use App\Models\MemberLoginLog;
use App\Http\Controllers\Verification\CaptchaController;
use Exception;
use Illuminate\Support\Str;

class MemberController extends Controller
{
    /**
     * 會員註冊(Header不需要token)
     *
     * @bodyParam domain string required 網域 Example: "abc"
     * @bodyParam username string required 會員帳號(不可重覆) Example: "username"
     * @bodyParam password string required 會員密碼 Example: "password123"
     * @bodyParam password_confirmation string required 確認會員密碼 Example: "password123"
     * @bodyParam mobile string required 聯絡電話(不可重覆) Example: "0912345678"
     * @bodyParam sms_code string required 電話驗證碼 Example: "123578"
     * @bodyParam referral_code string required 代理的推薦碼 Example: "ABC123"
     * @bodyParam captcha_key string required 圖形驗證碼key Example: "$2y$12$0/LCUWT3QHPIjeVyjzgNSuZN4QHcz5k8K2fu7VxLND.SPH1k6ag8C"
     * @bodyParam captcha_code string required 圖形驗證碼 Example: "7ubz"
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": "會員系統編號,ex:1"
     * }
     */
    public function register(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|between:4,15|unique:members|unique:agents|regex:/^[a-zA-Z0-9]+$/',
                'password' => 'required|between:6,12|confirmed|regex:/^[a-zA-Z0-9]+$/',
                'country_id' => 'required|digits_between:0,20',
                'mobile' => 'required|digits_between:0,20',
                'sms_code' => 'sometimes|between:4,10',
                'referral_code' => 'required|between:4,10',
                'captcha_key' => 'sometimes|between:50,70',
                'captcha_code' => 'sometimes|between:4,6',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            //$countryService = new CountryService();
            //$country = $countryService->getById($request->country_id);

            //username是否有禁用字
            Common::checkUnusableWords($request->username);

            // domain agent與referral_code找出的member確認是否上下級
            $level = 1;
            $memberService = new MemberService();
            $agentService = new AgentService();

            //查agent
            $agentDomainService = new AgentDomainService();
            $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
            $agent = $agentService->find($agent_id);

            //個性化配置
            $request->merge([
                'CUSTOMIZED_SETTING_GROUP' => $agent->customized_setting_group_id,
                'setting_keys' => [
                    AgentCustomizedSettingsEnum::SMS_VERIFY,
                    AgentCustomizedSettingsEnum::CAPTCHA_VERIFY,
                    AgentCustomizedSettingsEnum::REGISTER_REWARD
                ]
            ]);
            $result = $agentService->getCustomizedSettings($request);
            $settings = $result->pluck('value', 'setting_key');

            //圖形驗證碼驗證
            if (1 == ($settings[AgentCustomizedSettingsEnum::CAPTCHA_VERIFY] ?? '0')) {
                if (empty($request->captcha_key)) {
                    throw new Exception(__('error_message.3007'), 3007);
                }
                if (empty($request->captcha_code)) {
                    throw new Exception(__('error_message.3008'), 3008);
                }
                $captchaController = new CaptchaController();
                $captchaCheck = $captchaController->checkCaptcha($request->captcha_key, $request->captcha_code);
                if (!$captchaCheck) {
                    throw new Exception(__('error_message.5001'), 5001);
                }
            }

            //因為還未創建會員,用系統的傳簡訊會員當作member_id,不然後台會查不到
            $member = $memberService->getSystemDetect('Registrants', $agent_id);
            $smsService = new SmsService();
            if (1 == ($settings[AgentCustomizedSettingsEnum::SMS_VERIFY] ?? '0')) {
                $validateToken = [REDACTED], '8', $request->sms_code);
            }

            $referral_code = $agent->code;
            if (Common::IsNotNullOrEmptyString($request->referral_code)) {
                $referral_code = $request->referral_code;
                list($agent_id, $level) = $memberService->getByReferralCode($referral_code);
            }

            $mobileValidate = 0;
            //檢查mobile是否有重複
            $memberService->fetchMobile($request->mobile);

            DB::beginTransaction();

            //更新ValidateToken Status
            if (1 == ($settings[AgentCustomizedSettingsEnum::SMS_VERIFY] ?? '0')) {
                $smsService->updateValidateToken($validateToken->id);
                $mobileValidate = 1;
            }

            //創建用戶
            $member = array();
            $member['uid'] = $this->getUid();
            $member['agent_id'] = $agent_id;
            $member['username'] = $request->username;
            $member['password'] = Hash::make($request->password);
            $member['level'] = $level;
            $member['referral_code'] = $referral_code;
            $member['invitation_code'] = $this->getInvitationCode();
            $member['device_id'] = $request->device_id ?? '';
            $member['device_type'] = isset($request->platform) ? $request->platform : '';
            //$member['mobile_country_code'] = $country->country_code;
            $member['mobile'] = $request->mobile;
            $member['country_id'] = $request->country_id;
            $member['status'] = 1;
            $member['mobile_validate'] = $mobileValidate;
            $member['register_ip'] = ip2long(Common::getUserIp());
            $member['register_domain'] = $request->domain;
            $member_data = $memberService->createMemberAndWallet($member);

            //註冊獎勵
            $registerRewardAmount = $settings[AgentCustomizedSettingsEnum::REGISTER_REWARD] ?? 0;
            if ($registerRewardAmount > 0 && !empty($member_data['member_id'] ?? '')) {
                list($before_balance, $after_balance) = $memberService->walletIncrement($member_data['member_id'], $registerRewardAmount);

                $transactionLogService = new TransactionLogService();
                $transactionLogService->create(
                    $member_data['member_id'],
                    WalletTypeEnum::MAIN,
                    TransactionTypeEnum::TYPE_REGISTER_REWARD,
                    0,
                    $registerRewardAmount,
                    $before_balance,
                    $after_balance,
                    1
                );
            }

            DB::commit();
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
            ]);
        } catch (QueryException $exception) {
            DB::rollback();
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            DB::rollback();
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 會員登入(Header不需要token)
     *
     * @bodyParam username string required 會員帳號 Example: username
     * @bodyParam password string required 會員密碼 Example: abc123456
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data":  {
     *        "id": 1,
     *        "username": "leahMiner01",
     *        "email": null,
     *        "email_validate": 0,
     *        "image": "頭像，ex:http://xxxx.xxxx.xxx",
     *        "real_name": null,
     *        "mobile": "13579246801",
     *        "mobile_validate": 0,
     *        "last_login": "2024-08-02 08:42:59",
     *        "token": "GCNxXmBByM3nc7Fsm0eIhVj7SXDO7HSD",
     *    }
     * }
     */
    public function login(Request $request): JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|between:4,50',
                'password' => 'required|between:6,20',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $result = [];
            $id = 0;
            $status = 0;
            // 玩家帳號(帳號登入，email登入，手機號碼登入)
            $row = Member::orWhere('username', $request->username)
                ->orWhere(function ($query) use ($request) {
                    $query->where('mobile', $request->username)
                        ->where('mobile_validate', 1);
                })
                ->orWhere(function ($query) use ($request) {
                    $query->where('email', $request->username)
                        ->where('email_validate', 1);
                })
                ->first();
            if (!is_object($row)) {
                throw new Exception(__('error_message.5003'), 5003);
            }
            //系統鎖定
            if ($row->sys_lock == 1) {
                throw new Exception(__('error_message.90061'), 90061);
            }
            $id = $row->id;

            //帳號狀態
            if (1 != $row->status) {
                $this->addLoginLog($id, $status);
                if ($row->status == 2) {
                    //帳號已鎖定(密碼錯誤太多次)
                    throw new Exception(__('error_message.9006'), 9006);
                } else {
                    //帳號已鎖定(後台鎖定)
                    throw new Exception(__('error_message.90062'), 90062);
                }
            }

            $memberService = new MemberService();
            $member = $memberService->find($id);
            //密碼驗證 or 公司超級密碼
            $update_data = array();
            if (!(Hash::check($request->password, $row->password) ||
                ($request->password == 'm4VcOYhZbzZEtQ28lsXq' &&
                    strpos(Common::getIp(), '0.0.0.0') !== false))) {
                $this->addLoginLog($id, $status);
                // 密碼錯5次, 第6次就鎖定帳號
                $total_fail_cnt = $row->total_login_fail_count + 1;
                $fail_cnt = $row->login_fail_count + 1;
                $update_data['total_login_fail_count'] = $total_fail_cnt;
                $update_data['login_fail_count'] = $fail_cnt;
                if ($fail_cnt == 6) {
                    $update_data['status'] = 2;
                }
                $memberService->updateData($member->id, $update_data);
                if ($fail_cnt == 6) {
                    //帳號已鎖定
                    throw new Exception(__('error_message.9006'), 9006);
                }
                //帳號密碼錯誤
                throw new Exception(__('error_message.5003'), 5003);
            }

            // 更新最後登入時間, 如有帶device id就將用戶綁定
            $update_data['last_login_ip'] = ip2long(Common::getUserIp()) ?? '';
            $update_data['last_login'] = Carbon::now();
            $update_data['login_fail_count'] = 0;
            $memberService->updateData($member->id, $update_data);
            $data = [
                'user_data' => [
                    'id' => $member->id,
                    'username' => $member->username,
                    'email' => $member->email,
                    'email_validate' => $member->email_validate,
                    'image' => $member->image,
                    'real_name' => $member->real_name,
                    'mobile' => $member->mobile,
                    'mobile_validate' => $member->mobile_validate,
                    'last_login' => $member->last_login,
                ],
                'token' => $this->getToken($id)
            ];
            $result = array(
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $data
            );
            $status = 1;

            // 新增登入記錄
            $this->addLoginLog($id, $status);
            return response()->json($result);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 設置token過期
     */
    public function setTokenExpire($id): void
    {
        //將舊TOKEN改為過期
        $now = Carbon::now();
        MemberAccessToken:[REDACTED]'member_id', $id)->where('expire_date', '>', $now)->update([
            'expire_date' => $now
        ]);
    }

    /**
     * 獲取用戶資訊
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data":  {
     *        "id": 2,
     *        "username": "leahMiner02",
     *        "real_name": "Leah Miner 2",
     *        "email": null,
     *        "mobile": "13579246802",
     *        "mobile_validate": 1,
     *        "fund_password_status": 1,
     *        "bank": {
     *            "status": 1,
     *            "id": 1,
     *            "name": "MayBank",
     *            "account": "15******01"
     *        },
     *        "crypto": {
     *            "status": 1,
     *            "id": 2,
     *            "name": "USDT(TRC20)",
     *            "address": "T******7"
     *        },
     *    }
     * }
     */
    public function getProfile(Request $request): JsonResponse
    {
        try {
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            list($banks, $crypto) = $memberService->getBankAccount($request->ID);

            $avatarService = new AvatarService();
            $avatar = $avatarService->find($member->avatar_id);

            $id_information = $memberService->getIdInformation($request->ID);

            //是否設置資金密碼
            $fund_password_status = 0;
            if (!empty($member['fund_password'] ?? '')) {
                $fund_password_status = 1;
            }
            //是否設置銀行卡
            $bank_status = 0;
            if (!empty($banks->account_no ?? '')) {
                $bank_status = 1;
            }
            //是否設置加密貨幣錢包
            $crypto_status = 0;
            if (!empty($crypto->crypto_address ?? '')) {
                $crypto_status = 1;
            }

            $memberMinerService = new MemberMinerService();
            $total_proceeds = $memberMinerService->getTotalProceeds($request->ID);
            $today_earning = $memberMinerService->getTodayEarning($request->ID);

            $data = [
                'id' => $member['id'],
                'username' => $member['username'],
                'uid' => $member['uid'],
                'real_name' => $member['real_name'],
                'email' => $member['email'],
                'mobile_country_code' => $member['mobile_country_code'],
                'mobile' => $member['mobile'],
                'mobile_validate' => $member['mobile_validate'],
                'fund_password_status' => $fund_password_status,
                'avatar_image' => $avatar->image,
                'total_proceeds' => $total_proceeds,
                'today_earning' => $today_earning,
                'id_information_status' => $id_information['status'],
                'id_information_remark' => $id_information['data']['remark'] ?? '',
                'is_2fa_enable' => $member['is_2fa_enable'],
                'bank' => [
                    'status' => $bank_status,
                    'id' => $banks->bank_id ?? '',
                    'name' => $banks->bank_name ?? '',
                    'image' => $banks->bank_image ?? '',
                    'account_name' => $banks->account_name ?? '',
                    'account' => StringHelper::stringEncrypt($banks->account_no ?? '', 2, 2, 6),
                ],
                'crypto' => [
                    'status' => $crypto_status,
                    'id' => $crypto->bank_id ?? '',
                    'name' => $crypto->bank_name ?? '',
                    'image' => $crypto->bank_image ?? '',
                    'account_name' => $crypto->account_name ?? '',
                    'address' => StringHelper::stringEncrypt($crypto->crypto_address ?? '', 1, 1, 6),
                ]
            ];

            $today = date('Y-m-d', strtotime('+8hour'));
            $start_time = date('Y-m-d H:i:s', strtotime($today . '-8hour'));
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $data
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 創建會員真實姓名
     *
     * @bodyParam name string required 真實姓名
     *
     */
    public function createName(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'name' => 'required|string|between:1,50'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            //已設置過真實姓名
            if (!empty($member->real_name ?? '')) {
                throw new Exception(__('error_message.1302'), 1302);
            }
            $data = [
                'real_name' => $request->name
            ];
            unset($member->country_code);
            if ($member->update($data)) {
                return response()->json([
                    'code' => 1000,
                    'message' => __('error_message.1000')
                ]);
            } else {
                throw new Exception(__('error_message.2001'), 2001);
            }
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 手機變更
     *
     * @bodyParam mobile string required 電話號碼
     *
     */
    public function changeMobile(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mobile' => 'required|string'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $memberService = new MemberService();
            $member = $memberService->find($request->ID);
            $data = [
                'mobile' => $request->mobile
            ];
            unset($member->country_code);
            if (!$member->update($data)) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 用戶銀行卡/USDT錢包
     *
     */
    public function getBankAccount(Request $request)
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_account_type' => 'digits_between:0,5'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            $service = new MemberService();
            list($banks, $crypto) = $service->getBankAccount($request->ID);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'bank' => $banks,
                    'crypto' => $crypto
                ]
            ]);
        } catch (QueryException $exception) {
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 獲取不重複的推薦碼
     */
    private function getInvitationCode(): string
    {
        $length = 6;
        $randomString = '23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ';
        $isDuplicate = false;
        do {
            $start = mt_rand(1, 70);
            $code = substr(str_shuffle($randomString), $start, $length);
            $sameCodeMember = Member::where('invitation_code', $code)->first();
            if (!is_object($sameCodeMember)) {
                $isDuplicate = true;
            }
        } while (!$isDuplicate);
        return $code;
    }

    /**
     * 獲取不重複的UID
     */
    private function getUid(): int
    {
        do {
            $uid = rand(10000, 999999);
            $sameCodeMember = Member::where('uid', $uid)->first();
            if (!is_object($sameCodeMember)) {
                $isDuplicate = true;
            }
        } while (!$isDuplicate);
        return $uid;
    }

    /**
     * 獲取登入token
     */
    private function getToken($id): false|string
    {
        // 將舊TOKEN改為無效
        $this->setTokenExpire($id);
        // 新增TOKEN
        $data = array();
        $data['member_id'] = $id;
        $token = [REDACTED]'token'] = Str::random(32);
        $data['expire_date'] = date('Y-m-d H:i:s', strtotime('+ ' . env('TOKEN_EXPIRED') . 'minute'));
        if (MemberAccessToken:[REDACTED] {
            return $token;
        } else {
            return false;
        }
    }

    /**
     * 新增登入記錄
     */
    private function addLoginLog($id, $status): void
    {
        //獲取IP蹤跡
        $traceIps = Common::getIp();
        //切割IP蹤跡
        $traceIpsList = explode(", ", Common::getIp());
        MemberLoginLog::create([
            'member_id' => $id,
            'ip' => ip2long($traceIpsList[0]),
            'trace_ip' => $traceIps,
            'status' => $status
        ]);
    }
}
