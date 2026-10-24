<?php

namespace App\Repositories;

use App\Helpers\Common;
use Illuminate\Support\Facades\DB;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\RoleMenu;

class RoleMenuRepository extends BaseRepository
{
    public function model()
    {
        return RoleMenu::class;
    }

    public function getAccessList(object $params)
    {
        return DB::table('role_menus as rm')
            ->where('rm.role_id', $params->role_id)
            ->select(
                'menu_id'
            );
    }

    public function multiInsertMenu($params)
    {
        return DB::table('role_menus')->insert($params);
    }

    public function multiDeleteMenu($params)
    {
        return DB::table('role_menus')
            ->where('role_id', $params['role_id'])
            ->whereIn('menu_id', $params['menu_ids'])->delete();
    }
}
