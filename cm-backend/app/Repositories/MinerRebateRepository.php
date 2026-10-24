<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MinerRebate;

class MinerRebateRepository extends BaseRepository
{
    public function model(): string
    {
        return MinerRebate::class;
    }

    public function updateOrCreateByMinerId($params)
    {
        $attributes = [
            'miner_id' => $params['miner_id'],
        ];
        return $this->model->updateOrCreate($attributes, $params);
    }
}
