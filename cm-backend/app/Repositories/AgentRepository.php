<?php

namespace App\Repositories;

use App\Helpers\Common;
use Illuminate\Support\Facades\DB;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Agent;
use App\Models\Member;

/**
 * Class AgentRepository.
 */
class AgentRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Agent::class;
    }

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function findByUsername($username)
    {
        return $this->model->where('username', $username)->first();
    }

    public function findByInviteCode($code)
    {
        return $this->model->where('code', $code)->first();
    }

    public function getList(object $params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('agents as a')
            ->join('organizations AS o', function ($join) use ($params) {
                $join->on('o.id', 'a.organization_id')
                    ->when(Common::IsNotNullOrEmptyString($params->organization_id), function ($query) use ($params) {
                        $query->where('o.id', $params->organization_id);
                    });
            })
            ->join('roles AS r', function ($join) use ($params) {
                $join->on('r.id', 'a.role_id')
                    ->when(Common::IsNotNullOrEmptyString($params->role_id), function ($query) use ($params) {
                        $query->where('r.id', $params->role_id);
                    });
            })
            ->leftJoin('agents as pa', 'pa.id', 'a.parent')
            ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%')
            ->when(Common::IsNotNullOrEmptyString($params->manager_id), function ($query) use ($params) {
                $query->where('a.id', $params->manager_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->parent_name), function ($query) use ($params) {
                $query->where('pa.username', $params->parent_name);
            })
            ->when(Common::IsNotNullOrEmptyString($params->manager_name), function ($query) use ($params) {
                $query->where('a.username', $params->manager_name);
            })
            ->when(isset($params->status), function ($query) use ($params) {
                $query->where('a.status', $params->status);
            })
            ->orderBy('a.id', 'desc')
            ->select(
                'a.id',
                'a.username',
                DB::raw("IFNULL(pa.username, '') as parent_username"),
                'a.status',
                'a.created_at',
                DB::raw('INET_NTOA(a.last_login_ip) as last_login_ip'),
                'a.last_login_time',
                'o.id as organization_id',
                'o.name as organization_name',
                'r.id as role_id',
                'r.name as role_name',
            );
    }

    public function getAgent($id)
    {
        return DB::table('agents as a')
            ->join('organizations AS o', 'o.id', 'a.organization_id')
            ->join('agent_groups AS agc', 'agc.id', 'a.customized_setting_group_id')
            ->join('agent_groups AS agcf', 'agcf.id', 'a.cash_flow_group_id')
            ->join('agent_groups AS agsg', 'agsg.id', 'a.sms_gateway_group_id')
            ->join('agent_groups AS agm', 'agm.id', 'a.miner_group_id')
            ->join('roles AS r', 'r.id', 'a.role_id')
            ->where('a.id', $id)
            ->select(
                'a.id',
                'a.username',
                'a.customized_setting_group_id',
                'agc.name as customized_setting_group_name',
                'a.cash_flow_group_id',
                'agcf.name as cash_flow_group_name',
                'a.sms_gateway_group_id',
                'agsg.name as sms_gateway_group_name',
                'a.miner_group_id',
                'agm.name as miner_group_name',
                'a.status',
                'a.created_at',
                DB::raw('INET_NTOA(a.last_login_ip) as last_login_ip'),
                'a.last_login_time',
                'o.id as organization_id',
                'o.name as organization_name',
                'r.id as role_id',
                'r.name as role_name'
            )
            ->first();
    }

    public function updateLastLogin($id, $ip)
    {
        return $this->model
            ->where('id', $id)
            ->update([
                'last_login_ip' => ip2long($ip),
                'last_login_time' => date('Y-m-d H:i:s')
            ]);
    }

    public function getInvitationCode()
    {
        $length = 6;
        $randomString = '23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ';
        $isDuplicate = false;
        do {
            $start = mt_rand(1, 70);
            $code = substr(str_shuffle($randomString), $start, $length);
            $sameCodeAgent = $this->model->where('code', $code)->first();
            if (!is_object($sameCodeAgent)) {
                $isDuplicate = true;
            }

            $sameCodeMember = Member::where('invitation_code', $code)->first();
            if (!is_object($sameCodeMember)) {
                $isDuplicate = true;
            }
        } while (!$isDuplicate);
        return $code;
    }

    public function getCustomizedSettings($params)
    {
        return DB::table('agent_customized_settings')
            ->where('agent_group_id', $params->CUSTOMIZED_SETTING_GROUP)
            ->when(Common::IsNotNullOrEmptyString($params->setting_key ?? ''), function ($query) use ($params) {
                $query->where('setting_key', $params->setting_key);
            })
            ->when(isset($params->setting_keys), function ($query) use ($params) {
                $query->whereIn('setting_key', $params->setting_keys);
            });
    }
}
