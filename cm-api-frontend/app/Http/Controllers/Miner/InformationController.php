<?php

namespace App\Http\Controllers\Miner;

use Illuminate\Http\Request;
use Illuminate\Database\QueryException;
use App\Http\Controllers\Controller;
use App\Services\MinerService;
use Exception;
use Validator;

class InformationController extends Controller
{
    /**
     * home
     *
     * @response {
     *  "code": 1000,
     *  "message": "SUCCESS",
     *  "data": {
     *    }
     * }
     */
    public function getHome(Request $request)
    {
        try {
            $service = new MinerService();
            $topSell = $service->getTopSelling();
            $top10 = $service->getSalesRanking($request);

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'top_selling_miners' => $topSell,
                    'top_sales_people' => $top10
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
