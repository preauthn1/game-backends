<?php

namespace App\Services;

use App\Enums\PublicEnum;
use Carbon\Carbon;
use App\Repositories\TransactionLogRepository;
use Exception;

class TransactionLogService extends BaseService
{
    public function __construct()
    {
        $this->repository = new TransactionLogRepository();
    }

    public function create($member_id, $wallet_type_id, $transaction_type_id, $order_id, $point, $before_balance, $after_balance, $memo = '', $status = 0)
    {
        $data = array();
        $data['wallet_type_id'] = $wallet_type_id;
        $data['member_id'] = $member_id;
        $data['transaction_type_id'] = $transaction_type_id;
        $data['order_id'] = $order_id;
        $data['point'] = $point;
        $data['before_balance'] = $before_balance;
        $data['after_balance'] = $after_balance;
        $data['memo'] = $memo ?? '';
        if ($status != 0) {
            $data['status'] = $status;
        }

        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateDataByMemberId($id, $member_id, $data)
    {
        $result = $this->repository->updateDataByMemberId($id, $member_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function setSuccess($id, $member_id, $operator = 1)
    {
        $data = array();
        $data['status'] = 1;
        $data['operator'] = $operator;
        $this->updateDataByMemberId($id, $member_id, $data);
    }

    // 人工上下分
    public function getManualList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getManualList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->typeTxt = __('param.transaction_type.' . $item->type);

            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');

            $item->statusTxt = ($item->status == 1) ? __('param.success') : __('param.fail');
            return $item;
        });

        return [$total, $result];
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->transaction_type = __('param.transaction_type.' . $item->transaction_type_id);
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
    }

    // 人工上下分匯出
    public function getManualExport($params)
    {
        $model = $this->repository->getManualList($params)->get();
        $result = [];
        $model->map(function ($item) use (&$result) {
            $result[] = [
                'type' => __('param.transaction_type.' . ($item->type ?? 0)),
                'username' => $item->username ?? '',
                'amount' => $item->point ?? 0,
                'beforeAmount' => $item->before_balance ?? 0,
                'afterAmount' => $item->after_balance ?? 0,
                'createdAt' => Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s'),
                'operator' => $item->operator ?? '',
                'remark' => $item->memo ?? '',
            ];
        });
        return $result;
    }
}
