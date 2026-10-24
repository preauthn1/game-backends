<?php

namespace App\Services;

use App\Enums\PublicEnum;
use Carbon\Carbon;
use App\Repositories\AgentOperateLogRepository;
use App\Helpers\Common;
use Exception;

class AgentOperateLogService extends BaseService
{
    public function __construct()
    {
        $this->repository = new AgentOperateLogRepository();
    }

    public function create($params)
    {
        if (('OFF' == (env('OPERATION_LOG_EXCEPTION', 'OFF'))) && (2 == ($params['type'] ?? 1))) {
            //不記錄log
        } else {
            $createData = [
                'api_path' => $params['api_path'],
                'api_name' => $params['api_name'],
                'request' => $params['request'],
                'response_code' => $params['response_code'],
                'response_message' => $params['response_message'],
                'response_data' => $params['response_data'],
                'time_cost' => $params['time_cost'],
                'type' => $params['type'],
                'agent_id' => session('userData')->id,
                'agent_ip' => ip2long(Common::getUserIp()),
            ];
            $this->repository->create($createData);
        }
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        if (!empty($params->status ?? 0)) {
            $params->code_equ = (1 == $params->status ?? 0) ? '=' : '!=';
        }

        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            //文字過長自動縮短
            $item->request = strlen($item->request) > 30 ? mb_substr($item->request, 0, 30) . '...' : $item->request;
            $item->response_data = strlen($item->response_data) > 30 ? mb_substr($item->response_data, 0, 30) . '...' : $item->response_data;
            //轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            //狀態 成功或失敗
            $item->statusTxt = $item->response_code == PublicEnum::ResponseCode['success'] ? __('param.success') : __('param.fail');

            return $item;
        });

        return [$total, $result];
    }

    public function view($params)
    {
        $result = $this->repository->view($params);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        //轉換時區並格式化為 'Y-m-d H:i'
        $result->created_at = Carbon::parse($result->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
        $result->statusTxt = $result->response_code == PublicEnum::ResponseCode['success'] ? __('param.success') : __('param.fail');
        return $result;
    }
}
