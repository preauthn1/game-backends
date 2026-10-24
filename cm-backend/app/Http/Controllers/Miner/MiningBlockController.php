<?php

namespace App\Http\Controllers\Miner;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MiningBlockService;
use Exception;
use Illuminate\Support\Facades\Validator;

class MiningBlockController extends Controller
{
    /**
     * 挖礦難度 - 狀態
     */
    public function getStatusDropdown(Request $request)
    {
        try {
            //announcement.type.
            $list = array();
            $list[] = array('id' => 0, 'name' => __('web_message.status.pending'));
            $list[] = array('id' => 1, 'name' => __('web_message.general.approved'));
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
     * 挖礦難度 - 清單
     * 
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 10
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
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

            $service = new MiningBlockService();
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
     * 挖礦難度 - 詳情
     *
     * @bodyParam mining_block_id integer required 挖礦區塊ID Example: 1
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mining_block_id' => 'required|integer'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MiningBlockService();
            $data = $service->get($request->mining_block_id);

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
     * 挖礦難度 - 新增
     *
     * @bodyParam currency_id integer 挖礦幣種ID Example: 102
     * @bodyParam block_height integer required 區塊高度 Example: 14
     * @bodyParam block_time datetime 調整時間 Example: 2020-01-02 00:00
     * @bodyParam mining_difficulty decimal required 挖礦難度 Example: 57.32
     * @bodyParam mining_change decimal required 難度增幅 Example:0.35
     * @bodyParam block_reward decimal required 區塊獎勵 Example:0.35
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'currency_id' => 'required|integer|between:1,999999999',
                'block_height' => 'required|integer',
                'block_time' => 'required|date_format:Y-m-d H:i:s',
                'mining_difficulty' => 'required|numeric',
                'mining_change' => 'required|numeric',
                'block_reward' => 'required|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MiningBlockService();
            $service->create($request);

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
     * 挖礦難度 - 編輯
     *
     * @bodyParam mining_block_id integer required 礦機ID Example: 1
     * @bodyParam currency_id integer 挖礦幣種ID Example: 102
     * @bodyParam block_height integer required 區塊高度 Example: 14
     * @bodyParam block_time datetime 調整時間 Example: 2020-01-02 00:00
     * @bodyParam mining_difficulty decimal required 挖礦難度 Example: 57.32
     * @bodyParam mining_change decimal required 難度增幅 Example:0.35
     * @bodyParam block_reward decimal required 區塊獎勵 Example:0.35
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mining_block_id' => 'required|integer',
                'currency_id' => 'sometimes|integer|between:1,999999999',
                'block_height' => 'sometimes|integer',
                'block_time' => 'sometimes|date_format:Y-m-d H:i:s',
                'mining_difficulty' => 'sometimes|numeric',
                'mining_change' => 'sometimes|numeric',
                'block_reward' => 'sometimes|numeric'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MiningBlockService();
            $service->update($request);

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
     * 挖礦難度 - 更改狀態
     *
     * @bodyParam mining_block_id integer required 礦機ID Example: 1
     * @bodyParam status integer 狀態 0=待審核;1=審核通過; Example: 1
     */
    public function updateStatus(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mining_block_id' => 'required|integer',
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

            $service = new MiningBlockService();
            $service->updateStatus($request);

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
     * 挖礦難度 - 刪除
     *
     * @bodyParam mining_block_id integer required 礦機ID Example: 1
     */
    public function delete(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mining_block_id' => 'required|integer',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new MiningBlockService();
            $service->delete($request);

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
}
