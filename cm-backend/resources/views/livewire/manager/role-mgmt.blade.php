@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.manager.roleMgmt')}}</h1>
    </div>
    <div id="page-content">
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addRole">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>ID</th>
                            <th>{{__('web_message.general.roleName')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showModifyRole({{$data->id}})">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-danger" wire:click="showModifyRoleMenus({{$data->id}})">
                                        <i class="bi-journal-check"></i>
                                    </button>
                                </td>
                                <td>{{ $data->id }}</td>
                                <td>{{ $data->name }}</td>
                                <td>{{ $data->created_time }}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="4">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="4" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <!-- addRole -->
        <div class="modal fade" id="addRole" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.manager.addRole')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addRole">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.roleName')}}
                                    </label>
                                    <div class="col-sm-7">
                                        <input type="text" class="form-control" wire:model="addRoleName">
                                        <span>{{__('web_message.roleMgmt.addTip')}}</span>
                                    </div>
                                </div>

                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- modifyRole -->
        <div class="modal fade in" id="modifyRole" role="dialog"
             style="display: {{'modifyRole' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyRoleClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.manager.modifyRole')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModifyRole">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyRoleId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.roleName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyRoleName">
                                    </div>
                                </div>
                                @forelse((session()->get('pageCacheData')['accessGroupList']??[]) as $data)
                                    <div class="form-group">
                                        <label class="col-sm-4 control-label">
                                            {{$data->nameTxt}}
                                        </label>
                                        <div class="col-sm-6">
                                            @forelse(($data->sub??[]) as $sub)
                                                <label>
                                                    <input type="checkbox" wire:model="modifyRoleMenus"
                                                           value="{{$sub->id}}">
                                                    {{$sub->nameTxt}}
                                                </label>
                                            @empty
                                            @endforelse
                                        </div>
                                    </div>
                                @empty
                                @endforelse
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- modifyRoleMenus -->
        <div class="modal fade in" id="modifyRoleMenus" role="dialog"
             style="display: {{'modifyRoleMenus' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyRoleMenusClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.manager.modifyRoleMenus')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModifyRoleMenus">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyRoleMenusId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.roleName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyRoleMenusName" disabled>
                                    </div>
                                </div>
                                @forelse((session()->get('pageCacheData')['accessGroupList']??[]) as $data)
                                    <div class="form-group">
                                        <label class="col-sm-4 control-label">
                                            {{$data->nameTxt}}
                                        </label>
                                        <div class="col-sm-6">
                                            @forelse(($data->sub??[]) as $sub)
                                                <input id="inline-checkbox-{{$sub->id}}" class="magic-checkbox" type="checkbox" wire:model="modifyRoleMenus" value="{{$sub->id}}">
                                                <label for="inline-checkbox-{{$sub->id}}">{{$sub->nameTxt}}</label>
                                            @empty
                                            @endforelse
                                        </div>
                                    </div>
                                @empty
                                @endforelse
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#modifyRoleClose').click(function () {
                    $('#modifyRole').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
                $('#modifyRoleMenusClose').click(function () {
                    $('#modifyRoleMenus').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>

    </div>
@endsection
