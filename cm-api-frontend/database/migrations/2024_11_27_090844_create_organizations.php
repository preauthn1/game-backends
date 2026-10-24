<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'organizations';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci')->comment('組織名稱');
                $table->string('memo', 50)->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'name' => 'GFF', 'memo' => '預設組織'],
                ['id' => 2, 'name' => 'RD', 'memo' => 'RD總代理'],
                ['id' => 3, 'name' => 'QA', 'memo' => 'QA總代理'],
                ['id' => 4, 'name' => 'MyBit', 'memo' => 'MyBit總代理'],
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
