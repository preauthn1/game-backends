<?php

namespace App\Services;

use App\Enums\MinerEnum;
use App\Repositories\CurrencyRepository;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MinerRepository;
use App\Repositories\MinerLogRepository;
use App\Helpers\Common;
use Exception;

class MinerPurchaseService extends BaseService
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
        $miner = $this->repository->getMiner($id);
        if (!is_object($miner)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $miner->depreciation_cycle = $miner->depreciation_cycle / MinerEnum::CycleDays;
        $currencyRepository = new CurrencyRepository();
        $currency = $currencyRepository->getById($miner->currency_id);
        $miner->currency_name = $currency->name;
        $miner->miner_id = $miner->id;
        $miner->promo = json_decode($miner->promo);
        $miner->promo->purchase->cycle = ($miner->promo->purchase->days ?? 0) / MinerEnum::CycleDays;
        $miner->promo->group->cycle = ($miner->promo->group->days ?? 0) / MinerEnum::CycleDays;
        $miner->info = json_decode($miner->info);
        unset($miner->id);
        return $miner;
    }

    public function create($params)
    {
        $miner = $this->repository->findByName($params->name, $params->MINER_GROUP);
        if (is_object($miner)) {
            throw new Exception(__('error_message.5030'), 5030);
        }

        $data = array();
        $data['agent_group_id'] = $params->MINER_GROUP;
        $data['name'] = $params->name;
        $data['image'] = $params->image_url;
        $data['currency_id'] = $params->currency_id;
        $data['hash_rate'] = $params->hash_rate;
        $data['power_consumption'] = $params->power_consumption;
        $data['day_revenue'] = $params->day_revenue;
        $data['depreciation_cycle'] = $params->depreciation_cycle * MinerEnum::CycleDays;
        $data['purchase_price'] = $params->purchase_price;
        $data['purchase_total'] = $params->purchase_total;
        $data['rent_price'] = $params->rent_price;
        $data['group_price'] = $params->purchase_price * MinerEnum::GroupProportion;
        $data['group_total'] = $params->group_total;
        $purchase = array();
        $purchase['num'] = intval($params->promo_purchase_num);
        $purchase['days'] = intval($params->promo_purchase_days * MinerEnum::CycleDays);
        $group = array();
        $group['num'] = intval($params->promo_group_num);
        $group['days'] = intval($params->promo_group_days * MinerEnum::CycleDays);
        $rent = array();
        $rent['experience_commission'] = $params->experience_commission;
        $promo = array();
        $promo['purchase'] = $purchase;
        $promo['group'] = $group;
        $promo['rent'] = $rent;
        $data['promo'] = json_encode($promo);
        $data['sort'] = $params->order ?? 0;
        $data['is_show'] = $params->is_show;
        $data['status'] = $params->status;
        $info = array();
        $info['hash_rate'] = $params->hash_rate;
        $info['power_consumption'] = $params->power_consumption;
        $info['day_elec_fee'] = $params->day_elec_fee;
        $info['custody_fee'] = $params->custody_fee ?: 0;
        $info['custody_fee_rate'] = $params->custody_fee_rate ?: 0;
        $info['tag'] = $params->tag;
        $info['tag_color'] = $params->tag_color;
        $data['info'] = json_encode($info);
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $before = $this->repository->getById($params->miner_id);
        $info = json_decode($before->info, true);
        $promo = json_decode($before->promo, true);
        $memberMinerRepository = new MemberMinerRepository();
        $count = $memberMinerRepository->getCountByMinerId($params->miner_id, 1);
        $rent_count = $memberMinerRepository->getCountByMinerId($params->miner_id, 0);
        $group_count = $memberMinerRepository->getCountByMinerId($params->miner_id, 2);

        $data = array();
        if (Common::IsNotNullOrEmptyString($params->name)) {
            $miner = $this->repository->findByName($params->name, $params->MINER_GROUP);
            if (is_object($miner) && $miner->id != $params->miner_id) {
                throw new Exception(__('error_message.5030'), 5030);
            }
            $data['name'] = $params->name;
        }
        if (Common::IsNotNullOrEmptyString($params->image_url)) {
            $data['image'] = $params->image_url;
        }
        if (Common::IsNotNullOrEmptyString($params->hash_rate)) {
            $data['hash_rate'] = $params->hash_rate;
            $info['hash_rate'] = $params->hash_rate;
        }
        if (Common::IsNotNullOrEmptyString($params->power_consumption)) {
            $data['power_consumption'] = $params->power_consumption;
            $info['power_consumption'] = $params->power_consumption;
        }
        if (Common::IsNotNullOrEmptyString($params->day_revenue)) {
            $data['day_revenue'] = $params->day_revenue;
        }
        if (Common::IsNotNullOrEmptyString($params->purchase_total)) {
            if ($params->purchase_total < $count) {
                throw new Exception(__('error_message.5025'), 5025);
            }
            $data['purchase_total'] = $params->purchase_total;
        }
        if (Common::IsNotNullOrEmptyString($params->group_total)) {
            if ($params->group_total < $group_count) {
                throw new Exception(__('error_message.5025'), 5025);
            }
            $data['group_total'] = $params->group_total;
        }
        if (isset($params->is_show)) {
            $data['is_show'] = $params->is_show;
        }
        if (Common::IsNotNullOrEmptyString($params->status)) {
            if ($params->status == 0) {
                if ($count > 0) {
                    //目前有礦機被購買, 無法關閉
                    throw new Exception(__('error_message.5031'), 5031);
                } elseif ($rent_count > 0) {
                    //目前有礦機被租賃, 無法關閉
                    throw new Exception(__('error_message.5032'), 5032);
                } elseif ($group_count > 0) {
                    //目前有礦機被租賃, 無法關閉
                    throw new Exception(__('error_message.5033'), 5033);
                }
            }
            $data['status'] = $params->status;
        }
        if (isset($params->order)) {
            $data['sort'] = $params->order;
        }
        if (Common::IsNotNullOrEmptyString($params->day_elec_fee)) {
            $info['day_elec_fee'] = $params->day_elec_fee;
        }
        if (Common::IsNotNullOrEmptyString($params->custody_fee)) {
            $info['custody_fee'] = $params->custody_fee;
        }
        if (Common::IsNotNullOrEmptyString($params->custody_fee_rate)) {
            $info['custody_fee_rate'] = $params->custody_fee_rate;
        }
        if (Common::IsNotNullOrEmptyString($params->tag)) {
            $info['tag'] = $params->tag;
        }
        if (Common::IsNotNullOrEmptyString($params->tag_color)) {
            $info['tag_color'] = $params->tag_color;
        }
        $data['info'] = json_encode($info);

        if (Common::IsNotNullOrEmptyString($params->experience_commission)) {
            $promo['rent']['experience_commission'] = $params->experience_commission;
        }
        $data['promo'] = json_encode($promo);

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
