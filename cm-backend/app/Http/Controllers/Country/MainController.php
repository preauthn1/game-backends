<?php

namespace App\Http\Controllers\Country;

use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\CountryService;
use Illuminate\Http\Request;
use Exception;
use Illuminate\Support\Facades\Validator;

class MainController extends Controller
{
    public function getDropdown(Request $request)
    {
        try {
            $service = new CountryService();
            $list = $service->getDropdown();
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

    public function getStatusDropdown(Request $request)
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
     * 國家代碼列表
     *
     * @bodyParam isoCode string ISO代碼 Example: PE
     * @bodyParam countryCode string 國家代碼 Example: 51
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 10
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'isoCode' => 'sometimes|string|between:1,3',
                'countryCode' => 'sometimes|string|between:1,7',
                'status' => 'sometimes|integer|in:0,1',
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

            $service = new CountryService();
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
     * 國家代碼詳情
     *
     * @bodyParam id integer required 國家代碼ID Example: 33
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|between:1,999999999'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new CountryService();
            $data = $service->find($request->id);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'data' => $data
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
     * 新增國家代碼
     *
     * @bodyParam countryCode string required 國家代碼 Example: 51
     * @bodyParam isoCode string required ISO代碼 Example: PE
     * @bodyParam countryName string required 國家名稱 Example: Peru
     * @bodyParam phoneRule string required 手機號規則 Example: "^[0-9]\d{9}$"
     * @bodyParam order int 順序 Example: 1
     * @bodyParam status int required 狀態 Example: 1; 0:停用, 1:啟用
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'countryCode' => 'required|string|between:1,7',
                'isoCode' => 'required|string|between:1,3',
                'countryName' => 'required|string|between:1,100',
                'phoneRule' => 'required|string|between:1,100',
                'order' => 'sometimes|integer|between:0,99999',
                'status' => 'required|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new CountryService();
            $service->create($request);

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
     * 國家代碼修改
     *
     * @bodyParam id integer required 國家代碼ID Example: 33
     * @bodyParam countryCode string 國家代碼 Example: 51
     * @bodyParam isoCode string ISO代碼 Example: PE
     * @bodyParam countryName string 國家名稱 Example: Peru
     * @bodyParam phoneRule string 手機號規則 Example: "^[0-9]\d{9}$"
     * @bodyParam order int 順序 Example: 1
     * @bodyParam status int 狀態 Example: 1; 0:停用, 1:啟用
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|between:1,999999999',
                'countryCode' => 'sometimes|string|between:1,7',
                'isoCode' => 'sometimes|string|between:1,3',
                'countryName' => 'sometimes|string|between:1,100',
                'phoneRule' => 'sometimes|string|between:1,100',
                'order' => 'sometimes|integer|between:0,99999',
                'status' => 'sometimes|integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new CountryService();
            $service->update($request);

            $response = response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
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
