<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Task;
use Illuminate\Support\Facades\DB;

class TaskRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Task::class;
    }

    public function getList(object $params)
    {
        return DB::table('tasks as t')
            ->leftJoin('agents AS a', 'a.id', 't.operator')
            ->when(Common::IsNotNullOrEmptyString($params->command), function ($query) use ($params) {
                $query->where('t.command', 'like', '%' . $params->command . '%');
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('t.status', $params->status);
            })
            ->orderBy('t.id', 'desc')
            ->select(
                't.id',
                'a.id as operator_id',
                'a.username as operator',
                't.command',
                't.expression',
                't.description',
                't.status',
                't.created_at',
                DB::raw('IFNULL(t.updated_at, "") as updated_at'),
                DB::raw('IFNULL(t.last_run_at, "") as last_run_at'),
            );
    }

    public function view($params)
    {
        return DB::table('tasks as t')
            ->leftJoin('agents AS a', 'a.id', 't.operator')
            ->where('t.id', $params->id)
            ->select(
                't.id',
                'a.id as operator_id',
                'a.username as operator',
                't.command',
                't.expression',
                't.description',
                't.status',
                't.created_at',
                DB::raw('IFNULL(t.updated_at, "") as updated_at'),
            )
            ->first();
    }

    public function findByCommand($command)
    {
        return $this->model->where('command', $command)->first();
    }
}
