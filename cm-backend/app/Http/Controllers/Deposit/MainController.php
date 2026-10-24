<?php

namespace App\Http\Controllers\Deposit;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use Validator;

class MainController extends Controller
{
    /**
     * 充值訂單狀態下拉選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *          "list": [
     *              {
     *                  "id": 0,
     *                  "name": "Created"
     *              },
     *              ...
     *          ]
     *    }
     * }
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.deposit_order.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.deposit_order.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.deposit_order.status.2'));
            $list[] = array('id' => 3, 'name' => __('param.deposit_order.status.3'));

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
     * 充值渠道下拉選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *          "list": [
     *              {
     *                  "id": 1001,
     *                  "name": "Manual Recharge - Bank"
     *              },
     *              ...
     *          ]
     *    }
     * }
     */
    public function getGatewayDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1001, 'name' => __('param.deposit_gateway.1001'));
            $list[] = array('id' => 1002, 'name' => __('param.deposit_gateway.1002'));

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
     * 渠道類型下拉選單
     */
    public function getGatewayTypeDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.deposit_gateway.1001'));
            $list[] = array('id' => 3, 'name' => __('param.deposit_gateway.1002'));

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
     * 渠道狀態下拉選單
     */
    public function getGatewayStatusDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.disable'));
            $list[] = array('id' => 1, 'name' => __('param.enable'));
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
}
