<?php

namespace App\Http\Controllers\Miner;

use App\Helpers\MinerHelper;
use App\Services\MemberService;
use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MinerService;
use App\Services\MemberMinerService;
use Exception;
use Illuminate\Support\Facades\Validator;

class MinerController extends Controller
{
    /**
     * 銷售/租賃信息 - 礦機機型
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
            $minerService = new MinerService();
            $list = $minerService->getDropdown($request);

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
     * 會員礦機類型下拉選單
     */
    public function getTypeDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.miner.type.0'));
            $list[] = array('id' => 1, 'name' => __('param.miner.type.1'));
            $list[] = array('id' => 2, 'name' => __('param.miner.type.2'));
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
                ]
            ]);
        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }

    /**
     * 礦機-新增體驗
     *
     * @bodyParam miner_id integer 礦機ID Example: 1
     * @bodyParam username string 用戶帳號 Example: leahminer01
     */
    public function createExperience(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'miner_id' => 'required|digits_between:1,10|not_in:0',
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

            //會員資訊
            $memberService = new MemberService();
            $member = $memberService->findByUsername($request->username, true, $request->HIERARCHY_PATH);
            $request->member_id = $member->id;

            //礦機資訊
            $minerService = new MinerService();
            $miner = $minerService->getById($request->miner_id);
            if (empty($miner)) {
                throw new Exception(__('error_message.9001'), 9001);
            }
            $minerHelper = new MinerHelper();
            $minerPromo = $minerHelper->getPromo($miner->promo);
            if ($minerPromo['p_days'] < 1) {
                throw new Exception(__('error_message.5026'), 5026);
            }
            $request->info = '{"cycle_days":' . $minerPromo['p_days'] . '}';

            //操作人員
            $request->operator = $request->ID;

            $service = new MemberMinerService();
            $service->createExperience($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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
}
