<?php

namespace App\Services;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum;
use App\Repositories\MemberIdCardRepository;
use App\Helpers\Common;
use Carbon\Carbon;
use Exception;

class MemberIdCardService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberIdCardRepository();
    }

    public function find($id)
    {
        $result = $this->repository->find($id);
        if (!$result) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $result;
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

        $result = $result->map(function ($item) {
            $item->statusTxt = Common::IsNotNullOrEmptyString($item->status)
                ? __('param.member_id_card.status.' . $item->status)
                : __('param.member_id_card.status.9');
            $item->created_at = Carbon::parse($item->created_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            return $item;
        });
        return [$total, $result];
    }

    public function view($params)
    {
        $result = $this->repository->view($params);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        // 轉換時區並格式化為 'Y-m-d H:i'
        $result->created_at = Carbon::parse($result->created_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
        $result->updated_at = Carbon::parse($result->updated_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
        return $result;
    }

    public function updateStatusById($params)
    {
        $data = array();
        $data['status'] = $params->status;
        if (!empty($params->remark ?? '')) {
            $data['remark'] = $params->remark;
        }
        $data['operator'] = $params->ID;
        $result = $this->repository->updateById($params->member_id_card_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }
}
