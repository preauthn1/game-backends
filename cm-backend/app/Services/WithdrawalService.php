<?php

namespace App\Services;

use App\Enums\PublicEnum;
use Carbon\Carbon;
use App\Enums\TimeEnum;
use App\Helpers\Common;
use App\Repositories\WithdrawalOrderRepository;
use App\Repositories\BankRepository;
use Exception;

class WithdrawalService extends BaseService
{
    public function __construct()
    {
        $this->repository = new WithdrawalOrderRepository();
    }

    public function find($id)
    {
        $order = $this->repository->find($id);
        if (!$order) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $order;
    }

    public function createOrder($member_id, $order_no, $bank_id, $rate, $fee_type, $fee, $point, $ip, $cardno, $cardholder)
    {
        $finalFee = (1 == $fee_type) ? number_format($fee, 2) : number_format(bcmul($point, $fee, 5), 2);

        $data = array();
        $data['order_no'] = $order_no;
        $data['bank_id'] = $bank_id;
        $data['member_id'] = $member_id;
        $data['point'] = $point;
        $data['fee'] = $finalFee;
        $data['rate'] = $rate;
        $data['amount'] = bcmul(bcsub($point, $finalFee, 10), $rate, 10);
        $data['cardholder'] = $cardholder;
        $data['cardno'] = $cardno;
        $data['ip'] = $ip;

        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getOrder($order_id)
    {
        $order = $this->repository->getOrder($order_id);
        if (!$order) {
            throw new Exception(__('error_message.1524'), 1524);
        }

        $order->statusTxt = (in_array($order->status, [0, 1, 2, 3, 4]))
            ? __('param.withdrawal_order.status.' . $order->status)
            : __('param.other');

        if ($order->bank_type == 11) {
            $order->gateway_id = 1001;
            $order->gatewayTxt = __('param.withdrawal_gateway.1001');
        } elseif ($order->bank_type == 13) {
            $order->gateway_id = 1002;
            $order->gatewayTxt = __('param.withdrawal_gateway.1002');
        }
        $order->memberTypeTxt = __('param.member.type.' . $order->member_type);
        if(!empty($order->pay_time??'')){
            $order->pay_time = Carbon::parse($order->pay_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
        }

        return $order;
    }

    public function checkOrder($order_id, $status)
    {
        $order = $this->getOrder($order_id);
        //狀態 0:建立,1:成功,2:取消並退回,3:審核通過,4:取消不退回

        if ($status == 3) {  //待審核
            if ($order->status != 0) {
                //提領訂單非待審核
                throw new Exception(__('error_message.1525'), 1525);
            }
        } elseif ($status == 1) {
            if ($order->status != 3) {
                //提領訂單非已審核
                throw new Exception(__('error_message.1526'), 1526);
            }
        }
        return $order;
    }

    public function updateOrder($operator_id, $order_id, $status, $remark = '')
    {
        $result = $this->repository->updateOrder($operator_id, $order_id, $status, $remark);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateData($id, $data)
    {
        $result = $this->repository->updateById($id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateOrderStatus($id, $status)
    {
        $data = array();
        $data['status'] = $status;
        $result = $this->repository->updateById($id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getOrders($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;

        $model = $this->repository->getOrders($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->statusTxt = (in_array($item->status, [0, 1, 2, 3, 4]))
                ? __('param.withdrawal_order.status.' . $item->status)
                : __('param.other');

            if ($item->bank_type == 11) {
                $item->gateway_id = 1001;
                $item->gatewayTxt = __('param.withdrawal_gateway.1001');
            } elseif ($item->bank_type == 13) {
                $item->gateway_id = 1002;
                $item->gatewayTxt = __('param.withdrawal_gateway.1002');
            }
            $item->memberTypeTxt = __('param.member.type.' . $item->member_type);
            if(!empty($item->pay_time??'')){
                $item->pay_time = Carbon::parse($item->pay_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function getDataCollection($params)
    {
        $start_time7_db = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay()->subDays(6), TimeEnum::TimeZone['backend']);
        $start_time30_db = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay()->subDays(29), TimeEnum::TimeZone['backend']);
        $start_date7 = Carbon::parse($start_time7_db)->toDateString();
        $start_date30 = Carbon::parse($start_time30_db)->toDateString();
        $start_time7 = Carbon::parse($start_time7_db)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString();
        $start_time30 = Carbon::parse($start_time30_db)->setTimezone(TimeEnum::TimeZone['db'])->toDateTimeString();
        $data7 = $this->repository->getDataCollection($params, $start_time7);
        $data30 = $this->repository->getDataCollection($params, $start_time30);
        $withdrawal_list_7 = $this->transformData($data7, $start_date7, 7);
        $withdrawal_list_30 = $this->transformData($data30, $start_date30, 30);
        return [$withdrawal_list_7, $withdrawal_list_30];
    }

    private function transformData($data, $startDate, $days = 30)
    {
        // 初始化結果
        $result = [
            'date' => [],
            'count' => [],
            'amount' => []
        ];

        // 將data轉換為關聯數組，使用日期為鍵
        $dataByDate = [];
        foreach ($data as $item) {
            $dataByDate[$item->date] = (array) $item;
        }

        // 迭代從startDate開始的30天
        for ($i = 0; $i < $days; $i++) {
            $currentDate = date('Y-m-d', strtotime("+$i days", strtotime($startDate)));
            $formattedDate = date('Y/m/d', strtotime($currentDate));

            // 檢查data中是否有此日期的資料
            if (isset($dataByDate[$currentDate])) {
                $result['date'][] = $formattedDate;
                $result['count'][] = $dataByDate[$currentDate]['count'];
                $result['amount'][] = floatval($dataByDate[$currentDate]['amount']);
            } else {
                // 如果沒有此日期的資料，則填充0
                $result['date'][] = $formattedDate;
                $result['count'][] = 0;
                $result['amount'][] = 0;
            }
        }

        return $result;
    }

    public function getSummary($params, $field = '', $dateString = '', $method = 'count')
    {
        if (empty($field) || empty($method) || empty($dateString)) {
            return 0;
        }
        //資料欄位
        if ('member_id' == $field) {
            $field = 'distinct ' . $field;
        }
        //資料時間
        $nowMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now(), TimeEnum::TimeZone['backend']);
        $todayStartMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay(), TimeEnum::TimeZone['backend']);
        $todayEndMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->endOfDay(), TimeEnum::TimeZone['backend']);
        switch ($dateString) {
            case 'total':
                $startTimeUTC = '';
                $endTimeUTC = '';
                break;
            case 'today':
                $startTimeUTC = Carbon::parse($todayStartMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
                $endTimeUTC = Carbon::parse($todayEndMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
                break;
            default:
                $startTimeUTC = Carbon::parse($nowMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:00');
                $endTimeUTC = Carbon::parse($nowMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:59');
                break;
        }
        //資料方法
        if ('sum' == $method) {
            $result = $this->repository->getOrderSum($params, $field, $startTimeUTC, $endTimeUTC);
            return $result[0]->sum;
        } else {
            $result = $this->repository->getOrderCount($params, $field, $startTimeUTC, $endTimeUTC);
            return $result[0]->count;
        }

    }
}
