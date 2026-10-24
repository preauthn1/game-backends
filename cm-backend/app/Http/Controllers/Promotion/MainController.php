<?php

namespace App\Http\Controllers\Promotion;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\PromotionService;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Facades\Validator;

class MainController extends Controller
{

    public function getDropdown(Request $request)
    {
        try {
            //promotion.type.
            $list = array();
            $list[] = array('id' => 0, 'name' => __('param.promotion.type.0'));
            $list[] = array('id' => 1, 'name' => __('param.promotion.type.1'));
            $list[] = array('id' => 2, 'name' => __('param.promotion.type.2'));
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
     * 系統管理 - 活動設置
     *
     * @bodyParam type integer 活動類型 Example: 1, 0:預設(跳轉url), 1:登入頁彈窗, 2:內頁彈窗
     * @bodyParam status integer 狀態 Example: 1, 0:disable, 1:enable
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
     * @bodyParam page integer required 第幾頁 Example: 1
     * @bodyParam per_page integer required 每頁筆數 Example: 30
     */
    public function getList(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'integer|in:0,1,2',
                'status' => 'integer',
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

            $service = new PromotionService();
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
     * 系統管理 > 活動設置 > 明細
     *
     * @bodyParam promotion_id integer required ID Example: 1
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|digits_between:0,999999999'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new PromotionService();
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
     * 系統管理 > 活動設置 > 新增
     *
     * @bodyParam type integer required 類型 Example: 1
     * @bodyParam title string required 標題 Example: 標題1
     * @bodyParam content string 內容 Example: 內容1
     * @bodyParam image image 圖片 Example:
     * @bodyParam url string 跳轉網址 Example: http://example.invalid
     * @bodyParam start_time datetime required 開始時間 Example: 2024-01-01 00:00
     * @bodyParam end_time datetime required 結束時間 Example: 2024-01-01 00:00
     * @bodyParam status integer required 狀態 Example: 1
     */
    public function create(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'type' => 'required|integer|in:0,1,2',
                'title' => 'required|string|between:1,100',
                'content' => 'string',
                'url' => 'required_if:type,0|url',
                'image' => 'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'image_content' => 'required_if:type,2|regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'start_time' => 'required|date_format:Y-m-d H:i',
                'end_time' => 'required|date_format:Y-m-d H:i|after_or_equal:start_time',
                'status' => 'required|integer|in:0,1'
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
                $path = 'org/' . $request->ORGANIZATION_ID . '/promotion';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            if (Common::IsNotNullOrEmptyString($request->image_content)) {
                $path = 'org/' . $request->ORGANIZATION_ID . '/promotion';
                $image_url = CloudFlareHelper::uploadBase64File($request->image_content, $path);
                $request->image_content_url = $image_url;
            }

            $service = new PromotionService();
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
     * 系統管理 > 活動設置 > 修改
     *
     * @bodyParam type integer required 活動類型 Example: 1
     * @bodyParam title string 標題 Example: 標題1
     * @bodyParam content string 活動內容 Example: 活動內容1
     * @bodyParam image image 圖片 Example:
     * @bodyParam url string 跳轉網址 Example: http://example.invalid
     * @bodyParam start_time datetime 開始時間 Example: 2024-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2024-01-01 00:00
     * @bodyParam status integer 狀態 Example: 1
     */
    public function update(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer',
                'type' => 'integer|in:0,1,2',
                'title' => 'string|between:1,100',
                'content' => 'string',
                'image' => 'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'image_content' => 'regex:/^data:image\/(\w+);base64,([A-Za-z0-9+\/=]+)$/',
                'url' => 'url',
                'start_time' => 'date_format:Y-m-d H:i',
                'end_time' => 'date_format:Y-m-d H:i|after_or_equal:start_time',
                'status' => 'integer|in:0,1',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            if (Common::IsNotNullOrEmptyString($request->type)) {
                if ($request->type == 0 && Common::IsNullOrEmptyString($request->url) && Common::IsNullOrEmptyString($request->original_url)) {
                    throw new Exception(__('error_message.3035'), 3035);
                }

                if ($request->type == 2 && Common::IsNullOrEmptyString($request->image_content) && Common::IsNullOrEmptyString($request->original_image_content)) {
                    throw new Exception(__('error_message.3037'), 3037);
                }
            }

            if (Common::IsNotNullOrEmptyString($request->image)) {
                $path = 'org/' . $request->ORGANIZATION_ID . '/promotion';
                $image_url = CloudFlareHelper::uploadBase64File($request->image, $path);
                $request->image_url = $image_url;
            }

            if (Common::IsNotNullOrEmptyString($request->image_content)) {
                $path = 'org/' . $request->ORGANIZATION_ID . '/promotion';
                $image_url = CloudFlareHelper::uploadBase64File($request->image_content, $path);
                $request->image_content_url = $image_url;
            }

            $service = new PromotionService();
            $service->update($request);

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
