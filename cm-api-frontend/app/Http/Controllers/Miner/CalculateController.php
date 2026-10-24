<?php

namespace App\Http\Controllers\Miner;

use App\Enums\MinerEnum as MinerEnum;
use App\Enums\TimeEnum as TimeEnum;
use App\Helpers\MinerHelper;
use App\Http\Controllers\Controller;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MinerRepository;
use Carbon\Carbon;
use Illuminate\Database\QueryException;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Validator;
use Exception;

class CalculateController extends Controller
{
    /**
     * 礦機殘值試算
     *
     * @bodyParam mm_id int required 會員礦機ID
     * @bodyParam calculate_date date required 試算日期
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "residual_value": "1883.1944444444"
     *   }
     * }
     */
    public function getTrailRemain(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|digits_between:1,10',
                'calculate_date' => 'required|date',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            //會員礦機資訊
            $memberMinerRepository = new MemberMinerRepository();
            $myMinerDetail = $memberMinerRepository->getMyMinerDetail([
                'mm_id' => $request->mm_id,
                'member_id' => $request->ID
            ]);
            //無對應id紀錄
            if (empty($myMinerDetail)) {
                throw new Exception(__('error_message.5028'), 5028);
            }
            //試算日期不在max(可售回,昨天)往後半年之間
            $startDate = max($myMinerDetail['can_sellback_time'], Carbon::now()->subDay()->startOfDay()->toDateTimeString());
            $calculateStartDateMy = Carbon::createFromFormat('Y-m-d H:i:s', $startDate, TimeEnum::TimeZone['backend']);
            $calculateStartDateUtc = $calculateStartDateMy->setTimezone(TimeEnum::TimeZone['db']);
            $calculateEndDateMy = Carbon::parse($calculateStartDateMy)->setTimezone(TimeEnum::TimeZone['backend'])->startOfDay()->addDays(185);
            $calculateEndDateUtc = Carbon::parse($calculateEndDateMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString();
            $calculateTimeMy = Carbon::parse($request->calculate_date)->setTimezone(TimeEnum::TimeZone['backend'])->startOfDay();
            $calculateTimeUtc = Carbon::parse($calculateTimeMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString();
            if (!Carbon::parse($calculateTimeUtc)->between($calculateStartDateUtc, $calculateEndDateUtc, false)) {
                throw new Exception(__('error_message.5029') . '(1)', 5029);
            }
            //先使用礦機成本跟開始時間當作殘值跟最後折舊時間
            $residual_value = (MinerEnum::MinerType['group'] == ($myMinerDetail['type'] ?? 1)) ? $myMinerDetail['group_price'] ?? 0 : $myMinerDetail['purchase_price'] ?? 0;
            $lastDepreciableTime = '';
            if (!empty($myMinerDetail['start_time'])) {
                $minerStartMy = Carbon::createFromFormat('Y-m-d H:i:s', $myMinerDetail['start_time'], TimeEnum::TimeZone['backend']);
                $lastDepreciableTime = Carbon::parse($minerStartMy)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString();
            }
            if (empty($residual_value) || empty($lastDepreciableTime)) {
                throw new Exception(__('error_message.5028') . '(2)', 5028);
            }
            //抓取折舊紀錄 若有取最新那筆當作殘值跟最後折舊時間
            $minerRepository = new MinerRepository();
            $DepreciationList = $minerRepository->getDepreciationLast([
                'mm_id' => $request->mm_id,
                'calculate_date' => $calculateTimeUtc,
            ]);
            if (isset($DepreciationList[0]['residual_value'])) {
                $residual_value = $DepreciationList[0]['residual_value'] ?? 0;
                $lastDepreciableTime = $DepreciationList[0]['summary_time'] ?? '';
            }
            //計算當前時間跟最後折舊時間差異天數
            $aroundDays = Carbon::parse($lastDepreciableTime)->diffInDays($calculateTimeUtc);
            //差異天數大於折舊週期 計算差異週期 殘值-(差異週期*預估折舊金額)
            $depreciationCycle = $myMinerDetail['depreciation_cycle'] ?: MinerEnum::DefaultDepreciationDays;
            $diffRound = floor($aroundDays / $depreciationCycle);
            if ($aroundDays >= $depreciationCycle) {
                $minerHelper = new MinerHelper();
                $fixDepreciable = $minerHelper->fixDepreciable([
                    'mm_id' => $request->mm_id,
                    'member_id' => $request->ID
                ]);
                $residual_value = $residual_value - ($fixDepreciable * $diffRound);
            }
            //差異天數小於折舊週期 返回殘值
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'residual_value' => ($residual_value > 0) ? (string)$residual_value : '0'
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
     * 礦機殘值
     *
     * @bodyParam mm_id int required 會員礦機ID
     *
     * @response {
     *   "code": 1000,
     *   "message": "SUCCESS",
     *   "data": {
     *     "residual_value": "1883.1944444444"
     *   }
     * }
     */
    public function getRemain(Request $request): \Illuminate\Http\JsonResponse
    {
        try {
            //參數驗證
            $validator = Validator::make($request->all(), [
                'mm_id' => 'required|digits_between:1,10',
            ], $this->validatorMessage);
            if ($validator->fails()) {
                return response()->json([
                    'code' => 2002,
                    'message' => $validator->errors()->all()
                ]);
            }
            $minerHelper = new MinerHelper();
            $residual_value = $minerHelper->calculateRemain([
                'mm_id' => $request->mm_id,
                'member_id' => $request->ID
            ]);
            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'residual_value' => $residual_value
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
