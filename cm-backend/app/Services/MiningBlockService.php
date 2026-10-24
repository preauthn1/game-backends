<?php

namespace App\Services;

use App\Repositories\MiningBlockRepository;
use App\Helpers\Common;
use Exception;
use App\Enums\TimeEnum;
use Carbon\Carbon;

class MiningBlockService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MiningBlockRepository();
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
            $item->statusTxt = (1 == $item->status) ? __('web_message.general.approved') : __('web_message.status.pending');
            $item->block_time = Carbon::parse($item->block_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
            $item->mining_difficulty = floatval($item->mining_difficulty ?? 0);
            $item->mining_change = floatval($item->mining_change ?? 0);
            $item->block_reward = floatval($item->block_reward ?? 0);
            $item->created_at = Carbon::parse($item->created_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            $item->updated_at = Carbon::parse($item->updated_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
    }

    public function get($id)
    {
        $result = $this->repository->getById($id);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $result->block_time = Carbon::parse($result->block_time, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s');
        return $result;
    }

    public function create($params)
    {
        $data = array();
        $data['currency_id'] = $params->currency_id;
        $data['block_height'] = $params->block_height;
        $data['block_time'] = date('Y-m-d H:i:s', strtotime($params->block_time . '-8hour'));
        $data['mining_difficulty'] = $params->mining_difficulty;
        $data['mining_change'] = $params->mining_change;
        $data['block_reward'] = $params->block_reward;
        $data['status'] = 0;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $data = array();
        if (isset($params->currency_id)) {
            $data['currency_id'] = $params->currency_id;
        }
        if (isset($params->block_height)) {
            $data['block_height'] = $params->block_height;
        }
        if (isset($params->block_time)) {
            $data['block_time'] = date('Y-m-d H:i:s', strtotime($params->block_time . '-8hour'));
        }
        if (isset($params->mining_difficulty)) {
            $data['mining_difficulty'] = $params->mining_difficulty;
        }
        if (isset($params->mining_change)) {
            $data['mining_change'] = $params->mining_change;
        }
        if (isset($params->block_reward)) {
            $data['block_reward'] = $params->block_reward;
        }
        if (isset($params->status)) {
            $data['status'] = $params->status;
        }
        $result = $this->repository->updateById($params->mining_block_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
    }

    public function updateStatus($params)
    {
        $data = array();
        if (!isset($params->status)) {
            throw new Exception(__('error_message.5035'), 5035);
        }
        $data['status'] = $params->status;
        $data['operator'] = $params->ID;
        $result = $this->repository->updateById($params->mining_block_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
    }

    public function delete($params)
    {
        $result = $this->repository->deleteById($params->mining_block_id);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
