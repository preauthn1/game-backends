<?php

namespace App\Services;

use App\Repositories\MinerRebateRepository;
use App\Helpers\Common;
use Exception;

class MinerRebateService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MinerRebateRepository();
    }

    public function create($params)
    {
        $data = array();
        $data['miner_id'] = $params->miner_id;
        $data['percentage1'] = bcdiv($params->rebate_percentage_1, 100, 4);
        $data['percentage2'] = bcdiv($params->rebate_percentage_2, 100, 4);
        $data['percentage3'] = bcdiv($params->rebate_percentage_3, 100, 4);
        $data['status'] = 1;

        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $data = [
            'miner_id' => $params->miner_id,
        ];
        if (isset($params->rebate_percentage_1)) {
            $data['percentage1'] = bcdiv($params->rebate_percentage_1, 100, 4);
        }
        if (isset($params->rebate_percentage_2)) {
            $data['percentage2'] = bcdiv($params->rebate_percentage_2, 100, 4);
        }
        if (isset($params->rebate_percentage_3)) {
            $data['percentage3'] = bcdiv($params->rebate_percentage_3, 100, 4);
        }
        $result = $this->repository->updateOrCreateByMinerId($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
