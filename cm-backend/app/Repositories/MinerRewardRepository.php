<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MinerReward;

class MinerRewardRepository extends BaseRepository
{
    public function model(): string
    {
        return MinerReward::class;
    }

    public function insert($data)
    {
        return $this->model->insert($data);
    }

    public function updateOrCreateByMinerId($params)
    {
        $attributes = [
            'miner_id' => $params['miner_id'],
            'quantity' => $params['quantity'],
        ];
        $values = [
            'amount' => $params['amount'],
            'status' => 1,
        ];
        return $this->model->updateOrCreate($attributes, $values);
    }
}
