<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Helpers\Common;
use App\Models\MemberIdCard;
use DB;

/**
 * Class MemberIdCardRepository.
 */
class MemberIdCardRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return MemberIdCard::class;
    }

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function getLatestData(array $params): array
    {
        $model = MemberIdCard::orderBy('id', 'desc');
        if (!empty($params['member_id'] ?? '')) {
            $model->where('member_id', $params['member_id']);
        }
        $limit = ($params['limit'] ?? 0) ? $params['limit'] : 1;
        $model->limit($limit);
        return $model->get()->toArray();
    }

    public function getList($params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));

        return DB::table('member_id_cards as mic')
            ->join('members as m', function ($join) use ($params) {
                $join->on('m.id', 'mic.member_id')
                    ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                        $query->where('m.username', $params->username);
                    })
                    ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                        $query->where('m.uid', $params->uid);
                    });
            })
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftJoin('agents as o', 'o.id', 'mic.operator')
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('mic.status', $params->status);
            })
            ->when(Common::IsNotNullOrEmptyString($params->operator), function ($query) use ($params) {
                $query->where('o.username', $params->operator);
            })
            ->whereBetween('mic.created_at', [$start_time, $end_time])
            ->orderBy('mic.id', 'DESC')
            ->select(
                'mic.id as member_id_card_id',
                'mic.image_front as image_front',
                'mic.image_back as image_back',
                'mic.status',
                'mic.remark',
                DB::raw("IFNULL(o.username, '') as operator"),
                'm.uid',
                'm.username',
                'mic.created_at',
            );
    }

    public function view($params)
    {
        return DB::table('member_id_cards as mic')
            ->join('members as m', 'm.id', 'mic.member_id')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftJoin('agents as o', 'o.id', 'mic.operator')
            ->where('mic.id', $params->member_id_card_id)
            ->select(
                'm.uid',
                'm.username',
                'mic.id as member_id_card_id',
                'mic.image_front as image_front',
                'mic.image_back as image_back',
                'mic.status',
                'mic.remark',
                DB::raw("IFNULL(o.username, '') as operator"),
                'mic.created_at',
                'mic.updated_at',
            )
            ->first();
    }

}
