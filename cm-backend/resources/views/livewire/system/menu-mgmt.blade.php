@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.system.menuMgmt')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.menuMgmt.parentMenuName')}}：</label>
                        <select class="form-select form-control" wire:model="parentMenuId">
                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                            <option value="0">{{__('web_message.menuMgmt.mainMenu')}}</option>
                            @forelse((session()->get('pageCacheData')['parentMenuList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.menuMgmt.url')}}：</label>
                        <input type="text" class="form-control" wire:model="menuUrl">
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addNewMenu">
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
                            <th>{{__('web_message.menuMgmt.parentMenuName')}}</th>
                            <th>{{__('web_message.menuMgmt.menu')}}</th>
                            <th>{{__('web_message.menuMgmt.menuName')}}</th>
                            <th>{{__('web_message.menuMgmt.url')}}</th>
                            <th>{{__('web_message.menuMgmt.order')}}</th>
                            <th>{{__('web_message.menuMgmt.isShow')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showModify({{$data->id}})">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                </td>
                                <td>{{ $data->id }}</td>
                                <td>{{ $data->pNameTxt }}</td>
                                <td>{{ $data->name }}</td>
                                <td>{{ $data->nameTxt }}</td>
                                <td>{{ $data->uri }}</td>
                                <td>{{ $data->order }}</td>
                                <td style="font-size: 16px;"><i
                                        class="{{(1==$data->is_show)?'bi-toggle-on':'bi-toggle-off'}}"></i></td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="8">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="8" class="footable-visible">
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
        <!-- addNewMenu -->
        <div class="modal fade" id="addNewMenu" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.addMenu')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addNewMenu">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.menuMgmt.parentMenuName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <select class="form-select form-control" wire:model="addParentMenuId">
                                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                                            <option value="0">{{__('web_message.menuMgmt.mainMenu')}}</option>
                                            @forelse((session()->get('pageCacheData')['parentMenuList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.menuMgmt.menu')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addMenu">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.menuMgmt.url')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addMenuUrl">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.menuMgmt.order')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addMenuOrder">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.menuMgmt.isShow')}}
                                    </label>
                                    <div class="col-sm-6 lh-30">
                                        <input id="add-menu-isShow" class="toggle-switch" type="checkbox"
                                               wire:model="addMenuIsShow">
                                        <label for="add-menu-isShow"></label>
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
        <!-- modifyMenu -->
        <div class="modal fade in" id="modifyMenu" role="dialog"
             style="display: {{'modifyMenu' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.modifyMenu')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyMenuId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.menuMgmt.parentMenuName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <select class="form-select form-control" wire:model="modifyParentMenuId">
                                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                                            <option value="0">{{__('web_message.menuMgmt.mainMenu')}}</option>
                                            @forelse((session()->get('pageCacheData')['parentMenuList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.menuMgmt.menu')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyMenu">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.url')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyMenuUrl">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.order')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyMenuOrder">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.isShow')}}</label>
                                    <div class="col-sm-6 lh-30">
                                        <input id="modify-menu-isShow" class="toggle-switch" type="checkbox"
                                               wire:model="modifyMenuIsShow">
                                        <label for="modify-menu-isShow"></label>
                                    </div>
                                </div>
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
                $('#modifyClose').click(function () {
                    $('#modifyMenu').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
