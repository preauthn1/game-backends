<?php

namespace App\Http\Controllers\Verification;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use App\Http\Controllers\Controller;
use App\Helpers\TelegramHelper;
use App\Services\AgentDomainService;
use App\Services\AgentService;
use App\Services\CountryService;
use App\Services\MailService;
use App\Services\MemberService;
use App\Services\ValidateTokenService;
use App\Services\SmsService;
use App\Enums\ValidateTokenEnum;
use App\Enums\SmsGatewayEnum;
use App\Models\SimpleNamedLock;
use App\Models\SmsText;
use App\SmsApi;
use Illuminate\Support\Facades\Validator;
use Exception;
use DB;

class SmsCodeController extends Controller
{
    private $params = array(
        'insert_characters' => '',
        'left_credit_alert' => 0,
        'msg_merchant_prefix' => ''
    );

    /**
     * 獲取手機驗證碼
     *
     * @bodyParam domain string required 網域
     * @bodyParam type string required 驗證類型; 8:註冊(單頁式)
     * @bodyParam country_id string required 國碼
     * @bodyParam verify_by string required 驗證透過; 1:手機, 2:email
     * @bodyParam email string 郵箱
     * @bodyParam mobile string 電話號碼
     */
    public function getSmsCode(Request $request)
    {
        try {
            $rules = [
                'domain' => 'required|domain',
                'type' => 'required|in:1,2,3,4,5,6,7,8,9',
                'country_id' => 'required|digits_between:0,20',
                'verify_by' => 'required|in:1,2',
                //'email' => ($request->type == 2) ? 'email|between:0,60|unique:members,email' : 'email|between:0,60',
                'email' => 'email|between:0,60',
                //'mobile' => ($request->type == 1) ? 'digits_between:0,20|unique:members,mobile' : 'digits_between:0,20',
                'mobile' => 'digits_between:0,20|regex:/^[1-9][0-9]*$/',
            ];

            // 驗證POST資料
            $validator = Validator::make($request->all(), $rules, $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }

            //檢查前先Lock
            $lock = new SimpleNamedLock("ValidateTokenController", 0, $request->mobile);

            $req = $request->all();
            $req['sms_gateway_group_id'] = 0;
            $req['times'] = 1;
            $req['member_id'] = 0;
            $req['token'] = '';
            $req['country_code'] = '';

            //查組織ID
            $agentDomainService = new AgentDomainService();
            $agent_id = $agentDomainService->getAgentIdByDomain($request->domain);
            $agentService = new AgentService();
            $agent = $agentService->find($agent_id);
            $organization_id = is_object($agent) ? $agent->organization_id : 0;
            if ($organization_id == 0) {
                throw new Exception(__('error_message.9997'), 9997);
            }
            $req['sms_gateway_group_id'] = $agent->sms_gateway_group_id;

            $countryService = new CountryService();
            $country = $countryService->getById($request->country_id);
            $req['country_code'] = $country->country_code;

            $memberService = new MemberService();
            //檢查手機號是否重複: 註冊(一頁式)
            if (in_array($request->type, [8])) {
                $memberService->fetchMobile($request->mobile);
            }
            $smsService = new SmsService();
            list($token_expired, $max_send) = $smsService->getTokenExpired($agent->sms_gateway_group_id);
            if ($request->type == 1) {
                //註冊(多個步驟)
            } elseif ($request->type == 2) {
                //忘記密碼
            } elseif ($request->type == 3) {
                //找回密碼
            } elseif ($request->type == 4) {
                //洗分 驗證碼
            } elseif ($request->type == 5) {
                //支付密碼
            } elseif ($request->type == 6) {
                //忘記支付密碼,
            } elseif ($request->type == 7) {
                //洗分驗證,
            } elseif ($request->type == 8) {
                //註冊(一頁式)
                //因為還未創建會員,用系統的傳簡訊會員當作member_id,不然後台會查不到
                $member = $memberService->getSystemDetect('Registrants', $agent_id);
                $req['member_id'] = $member_id = $member->id;
            } elseif ($request->type == 9) {
                //9.修改綁定手機/email
            } else {
                throw new Exception(__('error_message.9991'), 9991);
            }

            //有效時間內($token_expired)的token不重新抓取，直接重新發送, 並用另一個簡訊商發送
            list($token, $times) = $memberService->getValidateTokenByMobile($token_expired, $max_send, $member_id, $request->mobile, $request->type, $request->verify_by);
            $req['times'] = $times;
            $req['token'] = $token;

            $result = $this->sendValidate($req);
            if (!$result) {
                // 發送失敗
                throw new Exception(__('error_message.2003'), 2003);
            }

            $lock->unLock();
            // 發送成功
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
            ]);
        } catch (QueryException $exception) {
            if(isset($lock)) {
                $lock->unLock();
            }
            $info = $exception->errorInfo;
            return response()->json([
                'code' => $info[0],
                'message' => $info[2]
            ]);
        } catch (Exception $exception) {
            if(isset($lock)) {
                $lock->unLock();
            }
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    private function sendValidate($req)
    {
        $return_value = true;
        $tg_message = '';
        if ($req['verify_by'] == 1) {
            //取得簡訊商發送順序
            $sms_gateways = DB::table('sms_gateway_infos as sgi')
                ->join('sms_gateways as sg', function ($join) {
                    $join->on('sg.id', 'sgi.sms_gateway_id')
                        ->where('sg.status', 1);
                })
                ->join('sms_gateway_info_countries as sgic', function ($join) {
                    $join->on('sgic.sms_gateway_info_id', 'sgi.id')
                        ->where('sgic.status', 1);
                })
                ->join('countries as c', function ($join) use ($req) {
                    $join->on('c.id', 'sgic.country_id')
                        ->where('c.status', 1)
                        ->whereRaw("'" . $req['country_code'] . $req['mobile'] . "' like concat(c.country_code, '%')");
                })
                ->where('sgi.status', 1)
                ->where('sgi.agent_group_id', $req['sms_gateway_group_id'])
                ->groupBy('sgi.id')
                ->orderBy('sgi.sort', 'ASC')
                ->select(
                    'sg.id as sms_gateway_id',
                    'sg.code',
                    'sg.url',
                    'example.invalid',
                    'c.id as country_id',
                    'c.country_code'
                )
                ->get();

            //當前可用簡訊商戶數
            $sms_gateway_count = count($sms_gateways);
            if ($sms_gateway_count > 0) {
                if ($sms_gateway_count == 1) {
                    $req['times'] = 1;
                } elseif ($req['times'] > $sms_gateway_count) {
                    //當前有效時間內發送次數 > 當前可用簡訊商戶數
                    $req['times'] = ($req['times'] % $sms_gateway_count);
                }
                $sms_gateway = $sms_gateways[($req['times'] - 1)];
                $sms_gateway = $this->setDefaultParamToSmsInfo($sms_gateway);

                // 取訊息
                $text = SmsText::where('agent_group_id', $req['sms_gateway_group_id'])
                    ->where('type_id', $req['type'])
                    ->where('country_id', $sms_gateway->country_id)
                    ->first();

                if (is_object($text)) {
                    //發送訊息
                    $token = [REDACTED]'token'], $sms_gateway);
                    $text = str_replace('{code}', $token, $text->text);
                    $type = 2;  //1:agent_id, 2:member_id
                    $sms = new SmsApi();
                    switch ($sms_gateway->code) {
                        case SmsGatewayEnum::CODE_LOCAL: //local
                            $result = $sms->sendByLocal($req['member_id'], $req['mobile'], $text, $sms_gateway);
                            break;
                        case SmsGatewayEnum::CODE_HUANXUN58: //huanxun58
                            $result = $sms->sendByHuanxun58($req['member_id'], $req['country_code'] . $req['mobile'], $text, $sms_gateway);
                            break;
                        case SmsGatewayEnum::CODE_QUANQIU: //quanqiu
                            $result = $sms->sendByQuanqiu($req['member_id'], $req['country_code'] . $req['mobile'], $text, $sms_gateway);
                            break;
                        case SmsGatewayEnum::CODE_ABOSEND: //abosend
                            $result = $sms->sendByAbosend($req['member_id'], $req['country_code'] . $req['mobile'], $text, $sms_gateway);
                            break;
                    }
                    if (!isset($result) || $result['code'] != 1000) {
                        $return_value = false;
                    }
                } else {
                    //未設定sms text
                    $tg_message = "【sms發送失敗-查無簡訊內容】" . PHP_EOL .
                        "mobile:" . $req['mobile'] . PHP_EOL .
                        "sms_group:" . $req['sms_gateway_group_id'] . PHP_EOL .
                        "SP: call ADMIN_check_sms_gateway('" . $req['mobile'] . "', " . $req['sms_gateway_group_id'] .
                        ", " . $req['type'] . ", " . $sms_gateway->country_id . ")";

                    $return_value = false;
                }
            } else {
                // 查無該國家簡訊商戶
                $tg_message = "【sms發送失敗-查無符合國際電話冠碼】" . PHP_EOL .
                    "mobile:" . $req['mobile'] . PHP_EOL .
                    "sms_group:" . $req['sms_gateway_group_id'] . PHP_EOL .
                    "SP: call ADMIN_check_sms_gateway('" . $req['mobile'] . "', " . $req['sms_gateway_group_id'] . ", 0)";

                $return_value = false;
            }
        } elseif ($req['verify_by'] == 2) {
            // mail 驗證
            if (count($this->sendValidateMail($req['email'], $req['token'])) > 0) {
                $return_value = false;
            }
        }

        if ($tg_message != '') {
            TelegramHelper::sendMessage($tg_message, 'sms');
        }
        return $return_value;
    }

    private function changeSmsContent($token, $sms_gateway)
    {
        $info = json_decode($sms_gateway->info, true);
        if (is_array($info) && $info['insert_characters'] != '') {
            //$token = [REDACTED] ($s, $info) {return $s.$info['insert_characters'];}, str_split($token)));
            $ar = str_split($token);
            foreach ($ar as $key => &$value) {
                $value = $value . (($key != array_key_last($ar)) ? $info['insert_characters'] : '');
            }
            $token = [REDACTED];
        }
        return $token;
    }

    private function setDefaultParamToSmsInfo($sms_gateway)
    {
        $info = $ori_info = json_decode($sms_gateway->info, true);

        if (!isset($info['insert_characters'])) {
            $info['insert_characters'] = $this->params['insert_characters'];
        }
        if (!isset($info['left_credit_alert'])) {
            $info['left_credit_alert'] = $this->params['left_credit_alert'];
        }
        if (!isset($info['prefix'])) {
            $info['prefix'] = $this->params['msg_merchant_prefix'];
        }

        if ($info != $ori_info) {
            $sms_gateway->info = json_encode($info);
        }
        return $sms_gateway;
    }
}
