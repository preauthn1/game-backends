<?php

namespace App\Services;

use App\Repositories\MinerRewardRepository;
use App\Helpers\Common;
use Exception;

class MinerRewardService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MinerRewardRepository();
    }

    public function create($params)
    {
        $result = $this->repository->create($params);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function multiInsert($params)
    {
        $data = [
            ['quantity' => 3, 'amount' => $params->reward_quantity_3, 'miner_id' => $params->miner_id, 'status' => 1],
            ['quantity' => 5, 'amount' => $params->reward_quantity_5, 'miner_id' => $params->miner_id, 'status' => 1],
            ['quantity' => 10, 'amount' => $params->reward_quantity_10, 'miner_id' => $params->miner_id, 'status' => 1],
        ];
        $result = $this->repository->insert($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $result = $this->repository->updateOrCreateByMinerId($params);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
