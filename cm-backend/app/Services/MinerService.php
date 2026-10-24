<?php

namespace App\Services;

use App\Helpers\MinerHelper;
use App\Repositories\CurrencyRepository;
use App\Repositories\MemberSalesRankingRepository;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MinerRepository;
use App\Repositories\MinerLogRepository;
use App\Helpers\Common;
use Exception;

class MinerService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MinerRepository();
    }

    public function getById($id)
    {
        return $this->repository->getById($id);
    }

    public function getDropdown($params)
    {
        $result = $this->repository->getDropdown($params)->get();
        return $result->map(function ($item) {
            $minerHelper = new MinerHelper();
            $item->promos = $minerHelper->getPromo($item->miner_promo);
            return $item;
        });
    }

    //ranking
    public function getSalesRanking($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $repository = new MemberSalesRankingRepository();
        $model = $repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            $item->typeTxt = __('param.member.type.' . $item->type);
            return $item;
        });
        return [$total, $result];
    }

    public function findSalesRanking($id)
    {
        $repository = new MemberSalesRankingRepository();
        $data = $repository->getByRankingId($id);
        if (!is_object($data)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $data;
    }

    public function insertSalesRanking($params)
    {
        $repository = new MemberSalesRankingRepository();
        $ranking = $repository->getByMemberId($params->member_id);
        $data = array();
        $data['quantity_sold'] = $params->qty;
        $data['rebate_amount'] = $params->rebate_amount;
        if (is_object($ranking)) {
            $result = $repository->updateById($ranking->id, $data);
        } else {
            $data['member_id'] = $params->member_id;
            $result = $repository->create($data);
        }

        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateSalesRanking($params)
    {
        $repository = new MemberSalesRankingRepository();
        $data = array();
        $data['quantity_sold'] = $params->qty;
        $data['rebate_amount'] = $params->rebate_amount;
        $result = $repository->updateById($params->ranking_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function deleteSalesRanking($params)
    {
        $repository = new MemberSalesRankingRepository();
        $result = $repository->deleteById($params->ranking_id);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function getPurchaseList($params)
    {
        //$memberWallet = $this->repository->getPurchaseList($member_id, $type_id);
        //if (!$memberWallet) {
        //    //查無錢包
        //    throw new Exception(__('error_message.1503'), 1503);
        //}
        //return $memberWallet;
    }

    public function getRentalList($params)
    {
        //$memberWallet = $this->repository->getRentalList($member_id, $type_id);
        //if (!$memberWallet) {
        //    //查無錢包
        //    throw new Exception(__('error_message.1503'), 1503);
        //}
        //return $memberWallet;
    }

}
