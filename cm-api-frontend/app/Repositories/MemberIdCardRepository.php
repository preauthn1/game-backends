<?php

namespace App\Repositories;

use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\MemberIdCard;

class MemberIdCardRepository extends BaseRepository
{
    public function model()
    {
        return MemberIdCard::class;
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

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function getLastByMemberId($member_id)
    {
        return $this->model
            ->where('member_id', $member_id)
            ->first();
    }
}
