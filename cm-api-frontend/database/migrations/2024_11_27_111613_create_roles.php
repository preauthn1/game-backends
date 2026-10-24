<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'roles';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci');
                $table->string('slug', 50)->collation('utf8mb4_unicode_ci');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique('name', 'uni_agent_roles_name');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'name' => 'Administrator', 'slug' => 'administrator'],
                ['id' => 11, 'name' => 'general agent', 'slug' => 'general agent'],
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
