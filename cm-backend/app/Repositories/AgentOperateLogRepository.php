<?php

namespace App\Repositories;

use App\Enums\PublicEnum;
use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\AgentOperateLog;
use Illuminate\Support\Facades\DB;

class AgentOperateLogRepository extends BaseRepository
{
    public function model()
    {
        return AgentOperateLog::class;
    }

    public function getList($params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        $model = DB::table('agent_operate_logs as aol')
            ->join('agents as a', function ($query) use ($params) {
                $query->on('a.id', 'aol.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%')
                    ->when(Common::IsNotNullOrEmptyString($params->manager_name), function ($query) use ($params) {
                        $query->where('a.username', $params->manager_name);
                    });
            })
            ->when(Common::IsNotNullOrEmptyString($params->api_path), function ($query) use ($params) {
                $query->where('aol.api_path', $params->api_path);
            })
            ->when(Common::IsNotNullOrEmptyString($params->api_name), function ($query) use ($params) {
                $query->where('aol.api_name', $params->api_name);
            })
            ->when(Common::IsNotNullOrEmptyString($params->code_equ), function ($query) use ($params) {
                $query->where('aol.response_code', $params->code_equ, PublicEnum::ResponseCode['success']);
            })
            ->whereBetween('aol.created_at', [$start_time, $end_time])
            ->orderBy('aol.id', 'desc')
            ->select(
                'aol.*',
                'a.username'
            );
        return $model;
    }

    public function view($params)
    {
        return DB::table('agent_operate_logs as aol')
            ->join('agents as a', 'a.id', 'aol.agent_id')
            ->where('aol.id', $params->id)
            ->select(
                'aol.*',
                'a.username',
            )
            ->first();
    }
}
