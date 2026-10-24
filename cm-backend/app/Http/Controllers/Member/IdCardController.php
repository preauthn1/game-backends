<?php

namespace App\Http\Controllers\Member;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\MemberIdCardService;
use App\Services\MemberService;
use Exception;
use Illuminate\Support\Facades\Validator;
use Illuminate\Support\Facades\DB;

class IdCardController extends Controller
{
    /**
     * 會員ID管理類別下拉選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *    }
     * }
     */
    public function getDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.member_id_card.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.member_id_card.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.member_id_card.status.2'));


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
     * 會員ID管理-清單
     *
     * @bodyParam uid int UID Example: 1
     * @bodyParam username string 會員帳號 Example: username
     * @bodyParam status int 狀態 Example: 1; 0:待審核, 1:已實名, 2:審核未通過,
     * @bodyParam operator string 操作人 Example: username
     * @bodyParam start_time datetime 創建開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 創建結束時間 Example: 2020-01-02 23:59
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
                'status' => 'in:0,1,2',
                'operator' => 'string|between:2,20',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
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

            $service = new MemberIdCardService();
            list($total, $result) = $service->getList($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $result,
                    'pagination' => $this->getPagination($total, $request->per_page, $request->page)
                ]
            ]);
        } catch (QueryException|Exception $exception) {
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
     * 會員ID管理-詳情
     *
     * @bodyParam member_id_card_id integer required 會員身份證明細ID Example: 1
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
                'member_id_card_id' => 'required|digits_between:0,10'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberIdCardService();
            $data = $service->view($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
                ]
            ]);
        } catch (QueryException|Exception $exception) {
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
     * 會員ID管理-審核
     *
     * @bodyParam member_id_card_id integer required 會員身份證ID Example: 1
     * @bodyParam status int 狀態 Example: 1; 1:通過, 2:拒絕
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
            $validator = Validator::make($request->all(), [
                'member_id_card_id' => 'required|digits_between:0,10',
                'status' => 'required|in:1,2',
                //'remark' => 'string'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MemberIdCardService();
            $memberIdCard = $service->find($request->member_id_card_id);
            if ($memberIdCard->status != 0) {
                throw new Exception(__('error_message.5035'), 5035);
            }

            DB::beginTransaction();
            $result = $service->updateStatusById($request);

            //如果成功, 將member_id_cards.id更新至members
            if ($request->status == 1) {
                $memberService = new MemberService();
                $memberService->updateMemberIdCardId($memberIdCard->member_id, $memberIdCard->id);
            }
            DB::commit();
            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                ]
            ]);
        } catch (QueryException|Exception $exception) {
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
