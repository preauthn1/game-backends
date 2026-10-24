<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'menus';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('parent_id')->default(0);
                $table->integer('order')->default(0);
                $table->string('name', 50)->collation('utf8mb4_unicode_ci');
                $table->string('symbol', 50)->collation('utf8mb4_unicode_ci')->nullable();
                $table->string('uri', 191)->collation('utf8mb4_unicode_ci')->nullable();
                $table->string('permission', 191)->collation('utf8mb4_unicode_ci')->nullable();
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
                $table->tinyInteger('is_show')->default(1)->comment('是否展示在選單 0=不展示; 1=展示;');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'parent_id' => 0, 'order' => 1, 'name' => 'dashboard', 'symbol' => 'bi-speedometer2', 'uri' => '', 'is_show' => 1,],
                ['id' => 2, 'parent_id' => 0, 'order' => 2, 'name' => 'system', 'symbol' => 'bi-gear', 'uri' => '', 'is_show' => 1,],
                ['id' => 3, 'parent_id' => 0, 'order' => 3, 'name' => 'member', 'symbol' => 'bi-person', 'uri' => '', 'is_show' => 1,],
                ['id' => 4, 'parent_id' => 0, 'order' => 4, 'name' => 'agent', 'symbol' => 'bi-people', 'uri' => '', 'is_show' => 1,],
                ['id' => 5, 'parent_id' => 0, 'order' => 5, 'name' => 'deposit', 'symbol' => 'bi-credit-card', 'uri' => '', 'is_show' => 1,],
                ['id' => 6, 'parent_id' => 0, 'order' => 6, 'name' => 'withdrawal', 'symbol' => 'bi-credit-card-2-back', 'uri' => '', 'is_show' => 1,],
                ['id' => 7, 'parent_id' => 0, 'order' => 7, 'name' => 'financialMgmt', 'symbol' => 'bi-cart4', 'uri' => '', 'is_show' => 1,],
                ['id' => 8, 'parent_id' => 0, 'order' => 8, 'name' => 'manager', 'symbol' => 'bi-person-gear', 'uri' => '', 'is_show' => 1,],
                ['id' => 9, 'parent_id' => 1, 'order' => 1, 'name' => 'dashboard', 'symbol' => '', 'uri' => '/dashboard', 'is_show' => 1,],
                ['id' => 10, 'parent_id' => 2, 'order' => 3, 'name' => 'depositSetting', 'symbol' => '', 'uri' => '/depositSetting', 'is_show' => 1,],
                ['id' => 11, 'parent_id' => 2, 'order' => 4, 'name' => 'withdrawalSetting', 'symbol' => '', 'uri' => '/withdrawalSetting', 'is_show' => 1,],
                ['id' => 12, 'parent_id' => 2, 'order' => 10, 'name' => 'announcement', 'symbol' => '', 'uri' => '/announcement', 'is_show' => 1,],
                ['id' => 13, 'parent_id' => 2, 'order' => 12, 'name' => 'rankingList', 'symbol' => '', 'uri' => '/rankingList', 'is_show' => 1,],
                ['id' => 14, 'parent_id' => 2, 'order' => 13, 'name' => 'minerSetting', 'symbol' => '', 'uri' => '/minerSetting', 'is_show' => 1,],
                ['id' => 15, 'parent_id' => 2, 'order' => 14, 'name' => 'groupOrderSetting', 'symbol' => '', 'uri' => '/groupOrderSetting', 'is_show' => 1,],
                ['id' => 16, 'parent_id' => 3, 'order' => 1, 'name' => 'memberList', 'symbol' => '', 'uri' => '/memberList', 'is_show' => 1,],
                ['id' => 17, 'parent_id' => 3, 'order' => 4, 'name' => 'manualOperation', 'symbol' => '', 'uri' => '/manualOperation', 'is_show' => 1,],
                ['id' => 18, 'parent_id' => 5, 'order' => 1, 'name' => 'depositList', 'symbol' => '', 'uri' => '/depositList', 'is_show' => 1,],
                ['id' => 19, 'parent_id' => 6, 'order' => 1, 'name' => 'withdrawalList', 'symbol' => '', 'uri' => '/withdrawalList', 'is_show' => 1,],
                ['id' => 20, 'parent_id' => 7, 'order' => 1, 'name' => 'purchaseList', 'symbol' => '', 'uri' => '/purchaseList', 'is_show' => 1,],
                ['id' => 21, 'parent_id' => 7, 'order' => 2, 'name' => 'rentList', 'symbol' => '', 'uri' => '/rentList', 'is_show' => 1,],
                ['id' => 22, 'parent_id' => 7, 'order' => 3, 'name' => 'groupList', 'symbol' => '', 'uri' => '/groupList', 'is_show' => 1,],
                ['id' => 23, 'parent_id' => 7, 'order' => 4, 'name' => 'depreciationInfo', 'symbol' => '', 'uri' => '/depreciationInfo', 'is_show' => 1,],
                ['id' => 24, 'parent_id' => 7, 'order' => 5, 'name' => 'rebateInfo', 'symbol' => '', 'uri' => '/rebateInfo', 'is_show' => 1,],
                ['id' => 25, 'parent_id' => 7, 'order' => 6, 'name' => 'inviteRewardInfo', 'symbol' => '', 'uri' => '/inviteRewardInfo', 'is_show' => 1,],
                ['id' => 26, 'parent_id' => 3, 'order' => 3, 'name' => 'profitAnalysis', 'symbol' => '', 'uri' => '/profitAnalysis', 'is_show' => 1,],
                ['id' => 27, 'parent_id' => 3, 'order' => 1, 'name' => 'memberDetail', 'symbol' => '', 'uri' => '/memberDetail', 'is_show' => 0,],
                ['id' => 28, 'parent_id' => 4, 'order' => 1, 'name' => 'agentDetail', 'symbol' => '', 'uri' => '/agentDetail', 'is_show' => 1,],
                ['id' => 30, 'parent_id' => 3, 'order' => 2, 'name' => 'idInformation', 'symbol' => '', 'uri' => '/idInformation', 'is_show' => 1,],
                ['id' => 31, 'parent_id' => 2, 'order' => 5, 'name' => 'loginPage', 'symbol' => '', 'uri' => '/loginPage', 'is_show' => 1,],
                ['id' => 32, 'parent_id' => 8, 'order' => 3, 'name' => 'operationLog', 'symbol' => '', 'uri' => '/operationLog', 'is_show' => 1,],
                ['id' => 33, 'parent_id' => 8, 'order' => 2, 'name' => 'managerList', 'symbol' => '', 'uri' => '/managerList', 'is_show' => 1,],
                ['id' => 34, 'parent_id' => 2, 'order' => 1, 'name' => 'menuMgmt', 'symbol' => '', 'uri' => '/menuMgmt', 'is_show' => 1,],
                ['id' => 38, 'parent_id' => 8, 'order' => 1, 'name' => 'roleMgmt', 'symbol' => '', 'uri' => '/roleMgmt', 'is_show' => 1,],
                ['id' => 39, 'parent_id' => 2, 'order' => 2, 'name' => 'groupDefine', 'symbol' => '', 'uri' => '/groupDefine', 'is_show' => 0,],
                ['id' => 40, 'parent_id' => 8, 'order' => 4, 'name' => 'groupMgmt', 'symbol' => '', 'uri' => '/groupMgmt', 'is_show' => 0,],
            ]);
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
