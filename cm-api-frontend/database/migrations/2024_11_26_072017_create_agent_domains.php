<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_domains';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('PK流水號');
                $table->integer('agent_id')->comment('代理系統編號');
                $table->string('domain', 191)->collation('utf8mb4_unicode_ci')->comment('網址名稱');
                $table->integer('template_id')->default(0)->comment('模版系統編號,0為公版');
                $table->tinyInteger('mobile_templates')->default(0)->comment('手機模版系統編號');
                $table->integer('pc_templates')->default(0)->comment('電腦模版系統編號');
                $table->string('logo_pc', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('電腦版Logo');
                $table->string('logo_mobile', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('手機版Logo');
                $table->string('icon', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('favorite icon');
                $table->string('add_home_image', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('添加主螢幕教學圖片網址');
                $table->string('title', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('網站名稱');
                $table->tinyInteger('type')->default(0)->comment('類型: 0:正常, 1:推廣用(因為可能會用到短網址, 故其他在回給前端可能不適合)');
                $table->tinyInteger('need_login')->default(1)->comment('是否可使用公開頁面');
                $table->string('info', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('其他資訊');
                $table->tinyInteger('status')->default(1)->comment('0未使用,1使用中');
                $table->integer('sort')->default(0)->comment('排序');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
                $table->integer('admin')->comment('更新者ID');

                //unique
                $table->unique('domain', 'uni_' . $this->table_name . '_' . 'domain');
            });

            DB::table($this->table_name)->insert([
                'agent_id' => 100001,
                'domain' => 'localhost',
                'template_id' => 1,
                'mobile_templates' => 1,
                'pc_templates' => 1,
                'title' => 'RD',
                'admin' => 0
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
