<?php

namespace App\Services;

use App\Enums\PublicEnum;
use Carbon\Carbon;
use App\Helpers\Common;
use App\Repositories\DepositOrderRepository;
use App\Repositories\BankRepository;
use App\Repositories\BankAccountRepository;
use App\Repositories\CurrencyRepository;
use App\Enums\DepositEnum;
use App\Enums\DepositOrderStatusEnum;
use App\Enums\TimeEnum;
use Exception;

class DepositService extends BaseService
{
    public function __construct()
    {
        $this->repository = new DepositOrderRepository();
    }

    public function create($agent_group_id, $member_id, $amount, $bank_id, $cardholder = '')
    {
        $ip = Common::getUserIp();
        $order_no = Common::getOrderNo('D');

        //取得幣種匯率
        $bankRepository = new BankRepository();
        $currency = $bankRepository->getCurrencyByBankId($agent_group_id, $bank_id);
        if (!$currency) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        $this->bankAccountRepository = new BankAccountRepository();
        $bankAccount = $this->bankAccountRepository->getBankAccountByBankId($agent_group_id, $bank_id);
        if (!$bankAccount) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        $data = array();
        $data['order_no'] = $order_no;
        $data['bank_id'] = $bank_id;
        $data['bank_account_id'] = $bankAccount->id;
        $data['member_id'] = $member_id;
        $data['amount'] = $amount;
        $data['rate'] = $currency->deposit_rate; //充值當下還是成功當下??
        $data['point'] = bcdiv($amount, $currency->deposit_rate, 10);
        $data['cardholder'] = $cardholder;
        $data['ip'] = ip2long($ip);
        $this->repository = new DepositOrderRepository();
        $order = $this->repository->create($data);
        if (!$order) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $order;
    }

    public function getOrders($params)
    {
        $page = $params->page ?? 1;
        $per_page = $params->per_page ?? 20;

        $model = $this->repository->getOrders($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            $item->statusTxt = (in_array($item->status, [0, 1, 2, 3]))
                ? __('param.deposit_order.status.' . $item->status)
                : __('param.other');

            if ($item->bank_type == 1) {
                $item->gateway_id = 1001;
                $item->gatewayTxt = __('param.deposit_gateway.1001');
            } elseif ($item->bank_type == 3) {
                $item->gateway_id = 1002;
                $item->gatewayTxt = __('param.deposit_gateway.1002');
            }
            if(!empty($item->pay_time??'')){
                $item->pay_time = Carbon::parse($item->pay_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
            }
            return $item;
        });
        return [$total, $result];
    }

    public function getOrder($order_id)
    {
        $order = $this->repository->getOrder($order_id);
        if (!$order) {
            throw new Exception(__('error_message.1521'), 1521);
        }

        // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
        if(!empty($order->pay_time??'')){
            $order->pay_time = Carbon::parse($order->pay_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
        }
        $order->created_at = Carbon::parse($order->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s');
        $order->updated_at = Carbon::parse($order->updated_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s');

        $order->statusTxt = (in_array($order->status, [0, 1, 2, 3]))
            ? __('param.deposit_order.status.' . $order->status)
            : __('param.other');

        if ($order->bank_type == 1) {
            $order->gateway_id = 1001;
            $order->gatewayTxt = __('param.deposit_gateway.1001');
        } elseif ($order->bank_type == 3) {
            $order->gateway_id = 1002;
            $order->gatewayTxt = __('param.deposit_gateway.1002');
        }

        return $order;
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
        $deposit_list_7 = $this->transformData($data7, $start_date7, 7);
        $deposit_list_30 = $this->transformData($data30, $start_date30, 30);
        return [$deposit_list_7, $deposit_list_30];
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

    public function checkOrder($order_id)
    {
        $order = $this->getOrder($order_id);
        if ($order->status != 3) {  //待審核
            throw new Exception(__('error_message.1523'), 1523);
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
