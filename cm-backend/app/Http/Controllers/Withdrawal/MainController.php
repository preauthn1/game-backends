<?php

namespace App\Http\Controllers\Withdrawal;

use App\Services\BankService;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;
use Illuminate\Support\Facades\Validator;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;

class MainController extends Controller
{
    /**
     * 提現訂單狀態下拉選單
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.withdrawal_order.status.0'));
            $list[] = array('id' => 1, 'name' => __('param.withdrawal_order.status.1'));
            $list[] = array('id' => 2, 'name' => __('param.withdrawal_order.status.2'));
            $list[] = array('id' => 3, 'name' => __('param.withdrawal_order.status.3'));
            $list[] = array('id' => 4, 'name' => __('param.withdrawal_order.status.4'));

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
     * 提領渠道下拉選單
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "id": 1
     *          "list": [
     *              {
     *                  "id": 1001,
     *                  "name": "Manual Withdrawal - Bank"
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
            $list[] = array('id' => 1001, 'name' => __('param.withdrawal_gateway.1001'));
            $list[] = array('id' => 1002, 'name' => __('param.withdrawal_gateway.1002'));

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
     * 提領手續費類型下拉選單
     */
    public function getFeeTypeDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.withdrawal_fee.type.0'));
            $list[] = array('id' => 1, 'name' => __('param.withdrawal_fee.type.1'));

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
            $list[] = array('id' => 11, 'name' => __('param.withdrawal_gateway.1001'));
            $list[] = array('id' => 13, 'name' => __('param.withdrawal_gateway.1002'));

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
     * 提現管理-渠道列表
     *
     * @bodyParam type integer 類別(0:全部, 11:銀行, 13:加密貨幣) Example: 1
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam account_alias string 渠道別名 Example: abc
     * }
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'sometimes|digits_between:1,2',
                'currency_id' => 'sometimes|integer',
                'account_alias' => 'sometimes|string|between:1,50',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            list($total, $result) = $service->getWithdrawalBankList($request);

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
     * 詳情
     *
     * @bodyParam bank_id integer required 銀行ID Example: 7
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|integer|between:1,999999999'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new BankService();
            $data = $service->withdrawalBankView($request);

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
     * 提現管理-新增提現銀行
     * 
     * @bodyParam type_id integer 類型ID 11=銀行;13=加密貨幣 Example: 1
     * @bodyParam currency_id integer 幣別ID Example: 1
     * @bodyParam bank_name string 銀行名稱 Example: MayBank
     * @bodyParam alias string 銀行別名 Example: MYD001
     * @bodyParam min_amount decimal(18,4) 最小金額 Example: 10
     * @bodyParam max_amount decimal(18,4) 最大金額 Example: 1000
     * @bodyParam image image 圖片 Example:
     * @bodyParam tax_type int 提現手續費類型 0=比率; 1=固定金額; Example:0
     * @bodyParam tax decimal 提現手續費 Example:0.011
     * @bodyParam sort integer 排序 Example: 2
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type_id' => 'required|integer|between:11,19',
                'currency_id' => 'required|digits_between:1,10',
                'bank_name' => 'required|string|between:2,20',
                'bank_code' => 'required|string|between:1,20',
                'min_amount' => 'required|numeric|between:0,9999999999.999',
                'max_amount' => 'required|numeric|between:0,9999999999.999',
                'image' => 'sometimes|regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'tax_type' => 'sometimes|digits_between:1,4',
                'tax' => 'sometimes|numeric',
                'sort' => 'sometimes|digits_between:1,5',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            if (Common::IsNotNullOrEmptyString($request->image)) {
                $path = 'org/' . $request->ORGANIZATION_ID. '/bank';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            $service = new BankService();
            $service->createBank($request);

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

    /**
     * 提現管理-修改提現銀行
     *
     * @bodyParam bank_id integer required 銀行ID Example: 7
     * @bodyParam alias string 銀行別名 Example: MYD001
     * @bodyParam min_amount decimal(18,4) 最小金額 Example: 10
     * @bodyParam max_amount decimal(18,4) 最大金額 Example: 1000
     * @bodyParam tax_type int 提現手續費類型 0=比率; 1=固定金額; Example:0
     * @bodyParam tax decimal 提現手續費 Example:0.011
     * @bodyParam sort integer 排序 Example: 2
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam image image required 圖片 Example:
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'bank_id' => 'required|integer|between:1,999999999',
                'alias' => 'sometimes|string|between:1,20',
                'min_amount' => 'sometimes|numeric|between:0,9999999999.999',
                'max_amount' => 'sometimes|numeric|between:0,9999999999.999',
                'tax_type' => 'sometimes|digits_between:1,4',
                'tax' => 'sometimes|numeric',
                'sort' => 'sometimes|digits_between:1,5',
                'status' => 'sometimes|integer|in:0,1',
                'image' => 'sometimes|regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            if (Common::IsNotNullOrEmptyString($request->image)) {
                $path = 'org/' . $request->ORGANIZATION_ID. '/bank';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            $service = new BankService();
            $service->update($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000')
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
}
