<?php

namespace App\Http\Controllers\Announcement;

use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use App\Services\AnnouncementService;
use App\Helpers\CloudFlareHelper;
use App\Helpers\Common;
use Exception;
use Illuminate\Support\Facades\Validator;

class MainController extends Controller
{

    public function getDropdown(Request $request)
    {
        try {
            //announcement.type.
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.announcement.type.1'));
            $list[] = array('id' => 2, 'name' => __('param.announcement.type.2'));
            $list[] = array('id' => 3, 'name' => __('param.announcement.type.3'));
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
     * 系統管理 - 平台公告
     *
     * @bodyParam type integer 公告類型 Example: 1, 0:全部, 1:系統, 2:優惠, 3:平台
     * @bodyParam status integer 狀態 Example: 1, 0:disable, 1:enable
     * @bodyParam start_time datetime 開始時間 Example: 2020-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2020-01-02 00:00
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
                'type' => 'required|integer|in:0,1,2,3',
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

            $service = new AnnouncementService();
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
     * 系統管理 > 平台公告 > 明細
     *
     * @bodyParam announcement_id integer required 公告ID Example: 1
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *        "announcement_id": 1,
     *        "type": 1,
     *        "title": "Admin",
     *        "content": "Admin",
     *        "is_top": 1,
     *        "start_time": "2024-01-31 00:00:00",
     *        "end_time": "2024-12-31 23:59:59",
     *        "status": 1
     *    }
     * }
     */
    public function view(Request $request)
    {
        $start = microtime(true);
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'id' => 'required|integer|between:1,1000'
            ], $this->validatorMessage);
            if ($validator->fails()) {
                $response = response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
                $this->saveOperationLogData(get_class($this), __FUNCTION__, $request->all(), $response->getData(), $start, false);
                return $response;
            }

            $service = new AnnouncementService();
            $data = $service->find($request->id);

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
     * 系統管理 > 平台公告 > 新增
     *
     * @bodyParam type integer required 公告類型 Example: 1
     * @bodyParam title string required 標題 Example: 標題1
     * @bodyParam content string 公告內容 Example: 公告內容1
     * @bodyParam image image 圖片 Example:
     * @bodyParam redirect_url string 跳轉網址 Example: http://example.invalid
     * @bodyParam is_top integer required 置頂 Example: 1
     * @bodyParam start_time datetime required 開始時間 Example: 2024-01-01 00:00
     * @bodyParam end_time datetime required 結束時間 Example: 2024-01-01 00:00
     * @bodyParam status integer required 狀態 Example: 1
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
                'type' => 'required|integer|in:1,2,3',
                'title' => 'required|string|between:4,100',
                'content' => 'required|string',
                'is_top' => 'required|integer|in:0,1',
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

            if (Common::IsNotNullOrEmptyString($request->file('image'))) {
                $path = 'org/' . $request->ORGANIZATION_ID. '/announcement';
                $image_url = CloudFlareHelper::uploadImage($request->file('image'), $path);
                $request->image_url = $image_url;
            }

            $service = new AnnouncementService();
            $service->create($request);

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

    /**
     * 系統管理 > 平台公告 > 新增
     *
     * @bodyParam type integer required 公告類型 Example: 1
     * @bodyParam title string 標題 Example: 標題1
     * @bodyParam content string 公告內容 Example: 公告內容1
     * @bodyParam image image 圖片 Example:
     * @bodyParam redirect_url string 跳轉網址 Example: http://example.invalid
     * @bodyParam is_top integer 置頂 Example: 1
     * @bodyParam start_time datetime 開始時間 Example: 2024-01-01 00:00
     * @bodyParam end_time datetime 結束時間 Example: 2024-01-01 00:00
     * @bodyParam status integer 狀態 Example: 1
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
                'id' => 'required|integer',
                'type' => 'integer|in:1,2,3',
                'title' => 'string|between:4,100',
                'content' => 'string',
                'image' => 'string',
                'redirect_url' => 'string',
                'is_top' => 'integer|in:0,1',
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

            if (Common::IsNotNullOrEmptyString($request->file('image'))) {
                $path = 'org/' . $request->ORGANIZATION_ID. '/announcement';
                $image_url = CloudFlareHelper::uploadImage($request->file('image'), $path);
                $request->image_url = $image_url;
            }

            $service = new AnnouncementService();
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
