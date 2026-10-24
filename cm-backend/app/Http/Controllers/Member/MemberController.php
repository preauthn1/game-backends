<?php

namespace App\Http\Controllers\Member;

use App\Models\MemberAccessToken;
use Carbon\Carbon;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use App\Http\Controllers\Controller;
use App\Services\MemberService;
use App\Services\CountryService;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class MemberController extends Controller
{
    /**
     * 會員 - 下拉式選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1,
     *      "list": [
     *          {
     *              "id": 1,
     *              "name": "Normal member"
     *          },
     *          ...
     *      ]
     *    }
     * }
     */
    public function getDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.member.type.1'));
            $list[] = array('id' => 2, 'name' => __('param.member.type.2'));
            $list[] = array('id' => 3, 'name' => __('param.member.type.3'));

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
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
     * 會員管理-清單
     *
     * @bodyParam uid int UID Example: 1
     * @bodyParam username string 會員帳號 Example: username
     * @bodyParam real_name string 真實姓名 Example: username
     * @bodyParam parent_username string 上級帳號 Example: username
     * @bodyParam fuzzy_query int 模糊查詢 Example: 1
     * @bodyParam country_id int 國碼ID Example: 1
     * @bodyParam mobile string 電話號碼 Example: 886912345678
     * @bodyParam invitation_code string 邀請碼 Example: ABC123
     * @bodyParam login_ip string 登入IP Example: 0.0.0.0
     * @bodyParam register_ip string 註冊IP Example: 0.0.0.0
     * @bodyParam type int 會員類型 Example: 1; 1:正式會員, 2:內部測試會員, 3:外部測試會員
     * @bodyParam account_no string 銀行卡 Example:
     * @bodyParam crypto_addr string 加密錢包地址 Example:
     * @bodyParam start_time datetime 註冊開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 註冊結束時間 Example: 2020-01-02 00:00
     * @bodyParam status int 狀態 1=正常;2=凍結;9=密碼錯誤凍結; Example:1
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1,
     *        "username": "Admin",
     *        "token": "roOAsDmfT9nP1Y9uKD8aMylk6HcYPlfz"
     *    }
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'uid' => 'digits_between:0,10',
                'username' => 'string|between:4,20',
                'parent_username' => 'string|between:4,20',
                'mobile' => 'string|between:8,12',
                'invitation_code' => 'string|between:6,8',
                'login_ip' => 'fuzzy_ip',
                'register_ip' => 'fuzzy_ip',
                'type' => 'digits_between:0,3',
                'account_no' => 'string',
                'crypto_addr' => 'string',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'status' => 'sometimes|integer|in:1,2,9',
                'page' => 'required|integer|between:1,1000',
                'per_page' => 'required|integer|between:1,10000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            list($total, $result) = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
                ]
            ]);
        } catch (QueryException | Exception $exception) {
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-詳情
     *
     * @bodyParam member_id integer required 會員帳號 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1,
     *        "username": "Admin",
     *        "token": "roOAsDmfT9nP1Y9uKD8aMylk6HcYPlfz"
     *    }
     * }
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'bank_id' => 'digits_between:0,10',
                'type' => 'in:0,1,2'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $member = $service->view($request->member_id, true);
            list($bank_account, $crypto_account) = $service->getBankAccount($request->member_id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'member' => $member,
                    'bank' => $bank_account,
                    'crypto' => $crypto_account
                ]
            ]);
        } catch (QueryException | Exception $exception) {
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-新增虛擬用戶
     *
     * @bodyParam type integer required 用戶類別 Example: 2, 2:內部測試會員, 3:外部測試會員
     * @bodyParam username string required 姓名 Example: abc
     * @bodyParam password string required 密碼 Example: 1
     * @bodyParam fund_password string required 資金密碼 Example: 1
     * @bodyParam parent_username string 上級玩家姓名 Example: aaa
     * @bodyParam country_id int 國碼 Example: 1
     * @bodyParam mobile string required 手機號 Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'required|integer|in:2,3',
                'username' => 'required|string|between:4,15|regex:/^[a-zA-Z0-9]+$/',
                'real_name' => 'required|string|between:1,50',
                'password' => 'required|string|between:6,12|regex:/^[a-zA-Z0-9]+$/',
                'fund_password' => 'required|string|between:6,12',
                'country_id' => 'digits_between:0,10',
                'mobile' => 'required|string|between:9,20'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $service->checkMemberExistsByUsername($request->username);
            if (isset($request->agent_id)) {
                $service->find($request->agent_id);
            }
            if ($request->password == $request->fund_password) {
                throw new Exception(__('error_message.6002'), 6002);
            }

            if (Common::IsNullOrEmptyString($request->country_id)) {
                $countryService = new CountryService();
                $request->country_id = $countryService->getCountryIdByMobile($request->mobile);
            }
            if (0 >= ($request->country_id ?? 0)) {
                throw new Exception(__('error_message.5040'), 5040);
            }

            DB::beginTransaction();
            $member = $service->createVirtual($request);
            DB::commit();

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'member' => $member
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-個人資料修改
     *
     * @bodyParam member_id integer required 會員ID Example: 1
     * @bodyParam real_name string 姓名 Example: guest
     * @bodyParam country_id int 國碼 Example: 1
     * @bodyParam mobile string 手機號 Example: 8869123456
     * @bodyParam email string 郵箱 Example: user@example.invalid
     * @bodyParam password string 登入密碼 Example: 123456
     * @bodyParam fund_password string 提現密碼 Example: 123456
     * @bodyParam crypto_id integer 加密幣別ID Example: 1
     * @bodyParam crypto_addr string USDT地址 Example: 0xabc123
     * @bodyParam bank_id integer 銀行ID Example: 1
     * @bodyParam bank_account string 銀行卡戶名 Example: guest
     * @bodyParam bank_no string 銀行卡帳號 Example: 123456
     * @bodyParam gauth_code string google驗證碼 Example: 123456
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $gauth_code_required = (Common::IsNotNullOrEmptyString($request->password)
                || Common::IsNotNullOrEmptyString($request->fund_password)
                || Common::IsNotNullOrEmptyString($request->crypto_addr)
                || Common::IsNotNullOrEmptyString($request->bank_no)
                || Common::IsNotNullOrEmptyString($request->bank_account)
            ) ? 'required|' : '';
            $crypto_required = (isset($request->crypto_id)) ? 'required|' : '';
            $bank_required = (isset($request->bank_id)) ? 'required|' : '';

            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'real_name' => 'string|between:1,50',
                'country_id' => 'digits_between:0,10',
                'mobile' => 'string|between:9,15',
                'email' => 'email',
                'password' => 'string|between:6,12|regex:/^[a-zA-Z0-9]+$/',
                'fund_password' => 'string|between:6,12',
                'crypto_id' => 'digits_between:0,10',
                'crypto_addr' => $crypto_required . 'string',
                'bank_id' => 'digits_between:0,10',
                'bank_account' => $bank_required . 'string',
                'bank_no' => $bank_required . 'string',
                'memo' => 'sometimes|string',
                'gauth_code' => $gauth_code_required . 'string|between:6,7'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            //TODO google驗證
            if ($gauth_code_required != '') {
                //修改需要輸入管理員綁定的谷歌驗證器驗證碼：重置登入密碼、重置提現密碼、USDT地址修改、銀行卡信息修改
            }

            $service = new MemberService();
            $member = $service->find($request->member_id, true);

            if (Common::IsNotNullOrEmptyString($request->password) && Common::IsNotNullOrEmptyString($request->fund_password)) {
                if ($request->password == $request->fund_password) {
                    throw new Exception(__('error_message.6002'), 6002);
                }
            }
            if (Common::IsNotNullOrEmptyString($request->password)) {
                if (Hash::check($request->password, $member->fund_password)) {
                    //密碼同資金密碼
                    throw new Exception(__('error_message.6003'), 6003);
                }
            }
            if (Common::IsNotNullOrEmptyString($request->fund_password)) {
                if (Hash::check($request->fund_password, $member->password)) {
                    //資金密碼同密碼
                    throw new Exception(__('error_message.6004'), 6004);
                }
            }

            if (Common::IsNotNullOrEmptyString($request->mobile)) {
                if (Common::IsNullOrEmptyString($member->country_id) && Common::IsNullOrEmptyString($request->country_id)) {
                    $countryService = new CountryService();
                    $request->country_id = $countryService->getCountryIdByMobile($request->mobile);
                }
            }
            if (0 >= ($request->country_id ?? 0)) {
                throw new Exception(__('error_message.5040'), 5040);
            }

            DB::beginTransaction();
            $result = $service->updateInfo($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => []
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-狀態修改
     *
     * @bodyParam member_id integer required 會員ID Example: 1
     * @bodyParam status integer 狀態 Example: 9
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function updateStatus(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'status' => 'required|integer|in:1,9',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $service->find($request->member_id);

            DB::beginTransaction();
            $service->updateStatus($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => []
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-密碼錯誤狀態修改
     *
     * @bodyParam member_id integer required 會員ID Example: 1
     * @bodyParam status integer 狀態 Example: 1
     * @bodyParam failCount integer 錯誤次數 Example: 0

     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function updatePasswrodLock(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
                'status' => 'required|integer|in:1,2',
                'failCount' => 'required|integer|in:0,6',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $service->find($request->member_id);

            DB::beginTransaction();
            $service->updatePasswrodLock($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => []
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 設置token過期
     */
    public function setTokenExpire(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:0,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $service->find($request->member_id);

            //將舊TOKEN改為過期
            $now = Carbon::now();
            MemberAccessToken:[REDACTED]'member_id', $request->member_id)->where('expire_date', '>', $now)->update([
                'expire_date' => $now
            ]);
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => []
            ]);
        } catch (QueryException | Exception $exception) {
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員取款數據
     *
     * @bodyParam username string require 會員帳號 Example: username
     */
    public function getWithdrawalData(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'username' => 'required|string|between:4,20',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $memberData = $service->findByUsername($request->username, true, $request->HIERARCHY_PATH);
            $request->merge(['member_id' => $memberData->id]);
            $data = $service->getWithdrawalData($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => $data
            ]);
        } catch (QueryException | Exception $exception) {
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }

    /**
     * 會員管理-重設谷歌認證
     *
     * @bodyParam member_id integer required 會員ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS"
     * }
     */
    public function resetGoogle2fa(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'member_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberService();
            $service->find($request->member_id);

            DB::beginTransaction();
            $service->resetGoogle2fa($request);
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => []
            ]);
        } catch (QueryException | Exception $exception) {
            DB::rollback();
            $isException = true;
            $response = response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
        $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, $isException ?? false);
        return $response;
    }
}
