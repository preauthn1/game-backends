<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_groups';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('PK流水號');
                $table->integer('type')->comment('1:基本個性化設置組別、2:財務組別  3:簡訊設置組別 4.聊天chatGroup相關');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->string('name', 191)->collation('utf8mb4_unicode_ci')->comment('備註顯示名稱(多語系程式碼處理)');
                $table->string('memo', 191)->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->tinyInteger('is_default')->comment('是否為預設組別');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('type', 'idx_' . $this->table_name . '_' . 'type');
                $table->index('organization_id', 'idx_' . $this->table_name . '_' . 'organization_id');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'type' => 1, 'organization_id' => 1, 'name' => '預設個性化分組', 'memo' => '個性化設置', 'is_default' => 1],
                ['id' => 2, 'type' => 2, 'organization_id' => 1, 'name' => '預設支付分組', 'memo' => '支付設置', 'is_default' => 1],
                ['id' => 3, 'type' => 3, 'organization_id' => 1, 'name' => '預設簡訊分組', 'memo' => '簡訊設置', 'is_default' => 1],
                ['id' => 11, 'type' => 1, 'organization_id' => 2, 'name' => 'RD總代理個性化分組', 'memo' => '個性化設置', 'is_default' => 0],
                ['id' => 12, 'type' => 2, 'organization_id' => 2, 'name' => 'RD總代理支付分組', 'memo' => '支付設置', 'is_default' => 0],
                ['id' => 13, 'type' => 3, 'organization_id' => 2, 'name' => 'RD總代理簡訊分組', 'memo' => '簡訊設置', 'is_default' => 0],
                ['id' => 14, 'type' => 1, 'organization_id' => 3, 'name' => 'QA總代理個性化分組', 'memo' => '個性化設置', 'is_default' => 0],
                ['id' => 15, 'type' => 2, 'organization_id' => 3, 'name' => 'QA總代理支付分組', 'memo' => '支付設置', 'is_default' => 0],
                ['id' => 16, 'type' => 3, 'organization_id' => 3, 'name' => 'QA總代理簡訊分組', 'memo' => '簡訊設置', 'is_default' => 0],
                ['id' => 17, 'type' => 1, 'organization_id' => 4, 'name' => 'MyBit總代理個性化分組', 'memo' => '個性化設置', 'is_default' => 0],
                ['id' => 18, 'type' => 2, 'organization_id' => 4, 'name' => 'MyBit總代理支付分組', 'memo' => '支付設置', 'is_default' => 0],
                ['id' => 19, 'type' => 3, 'organization_id' => 4, 'name' => 'MyBit總代理簡訊分組', 'memo' => '簡訊設置', 'is_default' => 0],
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
