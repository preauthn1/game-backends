<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberLoginLog;

/**
 * Class MemberLoginLogRepository.
 */
class MemberLoginLogRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberLoginLog::class;
    }

    public function getListByMemberId($member_id)
    {
        return $this->model->where('member_id', $member_id);
    }

    public function getListByIp($ip)
    {
        return $this->model->where('ip', ip2long($ip));
    }
}
