<?php

namespace App\Repositories;

use App\Helpers\Common;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Menu;
use Illuminate\Support\Facades\DB;

/**
 * Class MenuRepository.
 */
class MenuRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Menu::class;
    }

    public function getParentMenuDropdown()
    {
        return $this->model
            ->select(
                'id',
                'name',
            )
            ->where('parent_id', 0)
            ->orderBy('order')
            ->get();
    }

    public function getList(object $params)
    {
        return DB::table('menus as m')
            ->leftJoin('menus AS pm', 'pm.id', 'm.parent_id')
            ->when(Common::IsNotNullOrEmptyString($params->parent_id), function ($query) use ($params) {
                $query->where('m.parent_id', $params->parent_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->uri), function ($query) use ($params) {
                $query->where('m.uri', 'like', '%' . $params->uri . '%');
            })
            ->orderByRaw('m.parent_id, m.order')
            ->select(
                'm.id',
                'm.parent_id',
                DB::raw('IFNULL(pm.name, "") as parent_name'),
                'm.name',
                'm.uri',
                'm.order',
                'm.is_show',
            );
    }

    public function getAgentMenu($role_id, $menu_id = 0)
    {
        if ($menu_id == 0) {
            $sub = DB::table('role_menus as rm')
                ->join('menus', function ($join) {
                    $join->on('rm.menu_id', 'menus.id')
                        ->where('menus.parent_id', '>', 0);
                })
                ->whereRaw('rm.role_id = ' . $role_id)
                ->groupBy('menus.parent_id')
                ->select(
                    'menus.parent_id as menu_id'
                );

            $model = DB::table('menus')
                ->join(
                    DB::raw('(' . $sub->toRawSql() . ') as rm'),
                    'rm.menu_id', 'menus.id'
                )
                ->where('menus.parent_id', 0)
                ->orderBy('order')
                ->select(
                    'menus.id',
                    'menus.order',
                    'menus.name',
                    'menus.symbol',
                    'menus.uri',
                    'menus.is_show'
                )
                ->get();
        } else {
            $model = DB::table('menus')
                ->join('role_menus as rm', function ($join) use ($role_id) {
                    $join->on('rm.menu_id', 'menus.id')
                        ->where('rm.role_id', $role_id);
                })
                ->where('menus.parent_id', $menu_id)
                ->orderBy('order')
                ->select(
                    'menus.id',
                    'menus.order',
                    'menus.name',
                    'menus.symbol',
                    'menus.uri',
                    'menus.is_show'
                )
                ->get();
        }
        return $model;
    }
}
