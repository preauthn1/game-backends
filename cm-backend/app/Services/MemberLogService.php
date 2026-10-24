<?php

namespace App\Services;

use App\Repositories\MemberLoginLogRepository;
use App\Repositories\TransactionLogRepository;
use App\Repositories\AgentRepository;

class MemberLogService extends BaseService
{
    public function __construct()
    {

    }

    public function getIpLogList($params)
    {
        $repository = new MemberLoginLogRepository();
        $model = $repository->getListByMemberId($params->member_id);

    }

    public function getTransactionLogList($params)
    {

    }
}