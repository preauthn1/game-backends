<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'role_menus';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->integer('role_id');
                $table->integer('menu_id');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index(['role_id', 'menu_id'], 'idx_role_menus_role_id_menu_id');
            });

            DB::table($this->table_name)->insert([
                ['role_id' => 1, 'menu_id' => 9],
                ['role_id' => 1, 'menu_id' => 10],
                ['role_id' => 1, 'menu_id' => 11],
                ['role_id' => 1, 'menu_id' => 12],
                ['role_id' => 1, 'menu_id' => 13],
                ['role_id' => 1, 'menu_id' => 14],
                ['role_id' => 1, 'menu_id' => 15],
                ['role_id' => 1, 'menu_id' => 16],
                ['role_id' => 1, 'menu_id' => 17],
                ['role_id' => 1, 'menu_id' => 18],
                ['role_id' => 1, 'menu_id' => 19],
                ['role_id' => 1, 'menu_id' => 20],
                ['role_id' => 1, 'menu_id' => 21],
                ['role_id' => 1, 'menu_id' => 22],
                ['role_id' => 1, 'menu_id' => 23],
                ['role_id' => 1, 'menu_id' => 24],
                ['role_id' => 1, 'menu_id' => 25],
                ['role_id' => 1, 'menu_id' => 26],
                ['role_id' => 1, 'menu_id' => 27],
                ['role_id' => 1, 'menu_id' => 28],
                ['role_id' => 1, 'menu_id' => 30],
                ['role_id' => 1, 'menu_id' => 31],
                ['role_id' => 1, 'menu_id' => 32],
                ['role_id' => 1, 'menu_id' => 33],
                ['role_id' => 1, 'menu_id' => 34],
                ['role_id' => 1, 'menu_id' => 38],
                ['role_id' => 1, 'menu_id' => 39],
                ['role_id' => 1, 'menu_id' => 40],
                ['role_id' => 11, 'menu_id' => 9],
                ['role_id' => 11, 'menu_id' => 10],
                ['role_id' => 11, 'menu_id' => 11],
                ['role_id' => 11, 'menu_id' => 12],
                ['role_id' => 11, 'menu_id' => 13],
                ['role_id' => 11, 'menu_id' => 14],
                ['role_id' => 11, 'menu_id' => 15],
                ['role_id' => 11, 'menu_id' => 16],
                ['role_id' => 11, 'menu_id' => 17],
                ['role_id' => 11, 'menu_id' => 18],
                ['role_id' => 11, 'menu_id' => 19],
                ['role_id' => 11, 'menu_id' => 20],
                ['role_id' => 11, 'menu_id' => 21],
                ['role_id' => 11, 'menu_id' => 22],
                ['role_id' => 11, 'menu_id' => 23],
                ['role_id' => 11, 'menu_id' => 24],
                ['role_id' => 11, 'menu_id' => 25],
                ['role_id' => 11, 'menu_id' => 26],
                ['role_id' => 11, 'menu_id' => 27],
                ['role_id' => 11, 'menu_id' => 28],
                ['role_id' => 11, 'menu_id' => 30],
                ['role_id' => 11, 'menu_id' => 31],
                ['role_id' => 11, 'menu_id' => 32],
                ['role_id' => 11, 'menu_id' => 33],
                ['role_id' => 11, 'menu_id' => 34],
                ['role_id' => 11, 'menu_id' => 38],
                ['role_id' => 11, 'menu_id' => 39],
                ['role_id' => 11, 'menu_id' => 40],
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
