<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Member;
use DB;

/**
 * Class MemberRepository.
 */
class MemberRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Member::class;
    }

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function getTeams($invite_code)
    {
        return $this->model->whereIn('referral_code', $invite_code)
            ->where('status', 1)
            ->select(
                'id',
                'username',
                'invitation_code',
                'created_at'
                //DB::raw('DATE_ADD(created_at, INTERVAL 8 HOUR) as created_at')
            )
            ->get();
    }

    public function findByMobile($mobile, $validate=true)
    {
        return $this->model->where('mobile', $mobile)
            ->when($validate, function ($query) {
                $query->where('mobile_validate', 1);
            })
            ->first();
    }

    public function getByReferralCode($referral_code)
    {
        return $this->model->where('invitation_code', $referral_code)
            ->where('status', 1)
            ->first();
    }


    public function updateByData($id, $data)
    {
        return $this->model->where('id', $id)
            ->update($data);
    }

    public function getCountByMobile($mobile, $organization_id)
    {
        return $this->model
            ->join('agents as a', function ($join) use ($organization_id) {
                $join->on('a.id', 'members.agent_id')
                    ->where('a.organization_id', $organization_id);
            })
            ->where('members.mobile', $mobile)
            ->where('members.mobile_validate', 1)
            ->count();
    }

    public function getSystemDetect($username, $agent_id)
    {
        return $this->model->where('username', 'like', $username. '_%')
            ->where('agent_id', $agent_id)
            ->first();
    }

    public function getValidateByMobile($organization_id, $mobile)
    {
        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($organization_id) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.organization_id', $organization_id)
                    ->where('a.status', 1);
            })
            ->where('m.mobile', $mobile)
            ->where('m.mobile_validate', 1)
            ->whereIn('m.status', [1, 2])
            ->select('m.*')
            ->first();
    }

    public function getValidateByEmail($organization_id, $email)
    {
        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($organization_id) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.organization_id', $organization_id)
                    ->where('a.status', 1);
            })
            ->where('m.email', $email)
            ->where('m.email_validate', 1)
            ->whereIn('m.status', [1, 2])
            ->select('m.*')
            ->first();
    }

    public function getAccountByMemberIds($member_ids)
    {
        $member_ids = (substr($member_ids, 0, 1) == ',' ? substr($member_ids, 1) : $member_ids);
        $members = explode(',', $member_ids);
        return $this->model->whereIn('id', $members)
            ->select(
                'id',
                'username'
            )
            ->get();
    }
}
