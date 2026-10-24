<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_setting_defines';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->integer('define_id')->comment('PK，非自動流水號');
                $table->integer('parent_id')->default(0)->comment('若是type2，會記錄上層分類id');
                $table->tinyInteger('group_type')->comment('群組類型 1:個性化 2:支付 3:簡訊');
                $table->integer('organization_id')->default(1);
                $table->integer('is_visible')->default(1)->comment('是否後台可看到編輯');
                $table->tinyInteger('is_required')->default(0)->comment('是否為必填 0:非必填 1:必填');
                $table->text('language_text')->collation('utf8mb4_unicode_ci')->comment('多語系對應');
                $table->string('default_value', 200)->collation('utf8mb4_unicode_ci')->nullable()->comment('首次編輯要帶入的預設值');
                $table->string('data_type', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('資料類型');
                $table->tinyInteger('sort')->default(0)->comment('排序');
                $table->string('memo', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註說明');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('parent_id', 'idx_' . $this->table_name . '_' . 'parent_id');
                $table->index('group_type', 'idx_' . $this->table_name . '_' . 'group_type');
            });
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
