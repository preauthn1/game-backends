<?php

namespace App\Services;

use App\Repositories\CurrencyRepository;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MinerRepository;
use App\Repositories\MinerLogRepository;
use App\Helpers\Common;
use Exception;

class MinerGroupService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MinerRepository();
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        return [$total, $result];
    }

    public function get($id)
    {
        $columns = array(
            'id', 'name', 'image', 'currency_id', 'hash_rate', 'power_consumption', 'day_revenue',
            'depreciation_cycle', 'group_price', 'group_total', 'promo', 'status', 'info'
        );
        $miner = $this->repository->getById($id, $columns);
        if (!is_object($miner)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $currencyRepository = new CurrencyRepository();
        $currency = $currencyRepository->getById($miner->currency_id);
        $miner->currency_name = $currency->name;
        $miner->miner_id = $miner->id;
        $miner->promo = json_decode($miner->promo);
        $miner->info = json_decode($miner->info);
        unset($miner->id);
        return $miner;
    }

    public function update($params)
    {
        $before = $this->repository->getById($params->miner_id);
        $memberMinerRepository = new MemberMinerRepository();
        $count = $memberMinerRepository->getCountByMinerId($params->miner_id, 2);

        $data = array();
        if (Common::IsNotNullOrEmptyString($params->group_price) && $before->group_price == 0) {
            $data['group_price'] = $params->group_price;
        }
        if (Common::IsNotNullOrEmptyString($params->group_total)) {
            if ($params->group_total < $count) {
                throw new Exception(__('error_message.5025'), 5025);
            }
            $data['group_total'] = $params->group_total;
        }

        $result = $this->repository->updateById($params->miner_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        $after = $this->repository->getById($params->miner_id);
        $this->writeMinerLog($params->miner_id, $before, $after, $params->ID);
        return $result;
    }

    private function writeMinerLog($miner_id, $before, $after, $agent_id)
    {
        $repository = new MinerLogRepository();
        $data = array();
        $data['agent_id'] = $agent_id;
        $data['miner_id'] = $miner_id;
        $data['before_info'] = json_encode($before);
        $data['after_info'] = json_encode($after);
        $result = $repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}