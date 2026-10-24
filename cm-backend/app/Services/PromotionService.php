<?php

namespace App\Services;

use App\Enums\PromotionEnum;
use App\Repositories\PromotionRepository;
use App\Helpers\Common;
use Exception;
use Carbon\Carbon;

class PromotionService extends BaseService
{
    public function __construct()
    {
        $this->repository = new PromotionRepository();
    }

    public function find($id)
    {
        $data = $this->repository->getById($id);
        if (!is_object($data)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $data;
    }

    public function create($params)
    {
        $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
        $data = array();
        $data['miner_group_id'] = $params->MINER_GROUP;
        $data['type'] = $params->type;
        $data['title'] = $params->title;
        if (Common::IsNotNullOrEmptyString($params->content)) {
            $data['content'] = $params->content;
        }
        if (Common::IsNotNullOrEmptyString($params->image_url)) {
            $data['image'] = $params->image_url;
        }
        if (Common::IsNotNullOrEmptyString($params->image_content_url)) {
            $data['image_content'] = $params->image_content_url;
        }
        if (Common::IsNotNullOrEmptyString($params->url)) {
            $data['url'] = $params->url;
        }
        $data['start_time'] = $start_time;
        $data['end_time'] = $end_time;
        $data['status'] = $params->status;
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function update($params)
    {
        $promotion = $this->find($params->id);
        if (!is_object($promotion)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->type)) {
            $data['type'] = $params->type;
        }
        if (Common::IsNotNullOrEmptyString($params->title)) {
            $data['title'] = $params->title;
        }
        if (Common::IsNotNullOrEmptyString($params->content)) {
            $data['content'] = $params->content;
        }
        if (Common::IsNotNullOrEmptyString($params->image_url)) {
            $data['image'] = $params->image_url;
        }
        if (Common::IsNotNullOrEmptyString($params->image_content_url)) {
            $data['image_content'] = $params->image_content_url;
        }
        if (Common::IsNotNullOrEmptyString($params->url)) {
            $data['url'] = $params->url;
        }
        if (Common::IsNotNullOrEmptyString($params->start_time)) {
            $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
            $data['start_time'] = $start_time;
        }
        if (Common::IsNotNullOrEmptyString($params->end_time)) {
            $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
            $data['end_time'] = $end_time;
        }
        if (Common::IsNotNullOrEmptyString($params->sort)) {
            $data['sort'] = $params->sort;
        }
        if (Common::IsNotNullOrEmptyString($params->status)) {
            $data['status'] = $params->status;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
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
            $item->typeTxt = in_array($item->type, [0, 1, 2])
                ? __('param.promotion.type.' . $item->type)
                : __('param.other');

            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->start_time = Carbon::parse($item->start_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');
            $item->end_time = Carbon::parse($item->end_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i');

            $item->statusTxt = $item->status == 1 ? __('param.enable') : __('param.disable');

            return $item;
        });

        return [$total, $result];
    }
}
