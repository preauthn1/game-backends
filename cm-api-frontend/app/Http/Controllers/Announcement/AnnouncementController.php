<?php

namespace App\Http\Controllers\Announcement;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\AnnouncementService;
use Exception;

class AnnouncementController extends Controller
{
    /**
     * 公告(分系統/優惠/平台)
     *
     * @bodyParam announcement_type integer 公告類別; 0:全部, 1:系統, 2:優惠, 3:平台
     *
     */
    public function getList(Request $request)
    {
        try {
            $service = new AnnouncementService();
            $announcementList = $service->getList($request->ID, $request->announcement_type);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $announcementList
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
}
