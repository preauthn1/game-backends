<?php

namespace App\Services;

use App\Repositories\TaskRepository;
use Exception;
use App\Helpers\Common;
use App\Enums\TimeEnum;
use Carbon\Carbon;

class TaskService extends BaseService
{
    public function __construct()
    {
        $this->repository = new TaskRepository();
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 10;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->created_at = Carbon::parse($item->created_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
            $item->updated_at = !empty($item->updated_at ?? '') && !empty($item->operator ?? '') ? Carbon::parse($item->updated_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i') : '';
            $item->last_run_at = !empty($item->last_run_at ?? '') ? Carbon::parse($item->last_run_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i:s') : '';
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
        $result->created_at = Carbon::parse($result->created_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i');
        $result->updated_at = !empty($item->updated_at ?? '') && !empty($item->operator ?? '') ? Carbon::parse($result->updated_at, TimeEnum::TimeZone['db'])->setTimezone(TimeEnum::TimeZone['backend'])->format('Y-m-d H:i') : '';
        return $result;
    }

    public function create($params)
    {
        $data = array();
        $data['command'] = $params->command;
        $data['expression'] = $params->expression;
        $data['status'] = $params->status;
        $data['info'] = $params->info;
        if (Common::IsNotNullOrEmptyString($params->description)) {
            $data['description'] = $params->description;
        }
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function find($id)
    {
        $data = $this->repository->getById($id);
        if (!is_object($data)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $data;
    }

    public function findByCommand($command, $check = true)
    {
        $data = $this->repository->findByCommand($command);
        if ($check) {
            if (!is_object($data)) {
                throw new Exception(__('error_message.9001'), 9001);
            }
        }
        return $data;
    }

    public function update($params)
    {
        $task = $this->find($params->id);
        if (!is_object($task)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->expression)) {
            $data['expression'] = $params->expression;
        }
        if (Common::IsNotNullOrEmptyString($params->description)) {
            $data['description'] = $params->description;
        }
        if (isset($params->status)) {
            $data['status'] = $params->status;
        }
        if (isset($params->info)) {
            $data['info'] = $params->info;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateById($params->id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }
}
