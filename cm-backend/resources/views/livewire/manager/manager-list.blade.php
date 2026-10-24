@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.manager.managerList')}}</h1>
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
                        <label>ID：</label>
                        <input type="text" class="form-control" wire:model="managerId">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.organization')}}：</label>
                        <select class="form-select form-control" wire:model="managerOrganizationId">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['organizationList']??[]) as $data)
                                <option value="{{$data->organization_id}}">{{$data->organization_name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.agentName')}}：</label>
                        <input type="text" class="form-control" wire:model="parentName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.managerName')}}：</label>
                        <input type="text" class="form-control" wire:model="managerName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.role')}}：</label>
                        <select class="form-select form-control" wire:model="managerRoleId">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['roleList']??[]) as $data)
                                <option value="{{$data->role_id}}">{{$data->role_name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="managerStatus">
                            <option value="-1" selected>{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('param.enable')}}</option>
                            <option value="0">{{__('param.disable')}}</option>
                        </select>
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
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addManager">
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
                            <th>{{__('web_message.general.status')}}</th>
                            <th>ID</th>
                            <th>{{__('web_message.general.organization')}}</th>
                            <th>{{__('web_message.general.agentName')}}</th>
                            <th>{{__('web_message.general.managerName')}}</th>
                            <th>{{__('web_message.general.role')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.lastLoginIp')}}</th>
                            <th>{{__('web_message.general.lastLoginTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning"
                                            wire:click="showDataPop({{$data->id}},'modify')">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-danger"
                                            wire:click="showDataPop({{$data->id}},'detail')">
                                        <i class="bi-list-task"></i>
                                    </button>
                                </td>
                                <td>{{ $data->statusTxt }}</td>
                                <td>{{ $data->id }}</td>
                                <td>{{ $data->organization_name }}</td>
                                <td>{{ $data->parent_username }}</td>
                                <td>{{ $data->username }}</td>
                                <td>{{ $data->role_name }}</td>
                                <td>{{ $data->created_at }}</td>
                                <td>{{ $data->last_login_ip }}</td>
                                <td>{{ $data->last_login_time }}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="10">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="10" class="footable-visible">
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
        <!-- addMember -->
        <div class="modal fade" id="addManager" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.manager.addManager')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addManager">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addManagerUsername">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.password')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addManagerPassword">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.agentName')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addManagerAgentName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.referralCode')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addManagerReferralCode">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.role')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="addManagerRoleId">
                                            <option value="0">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['roleList']??[]) as $data)
                                                <option value="{{$data->role_id}}">{{$data->role_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.subAccount')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="modify-manager-status" class="toggle-switch" type="checkbox"
                                               wire:model="addManagerSubAccount">
                                        <label for="modify-manager-status"></label>
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
        <!-- dataPop -->
        <div class="modal fade in" id="dataPop" role="dialog"
             style="display: {{'dataPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;margin: 15px 0;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.manager.modifyManager'):__('web_message.menu.manager.managerDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body" style="padding-bottom: 10px;">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">ID</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyManagerId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-3 control-label">{{__('web_message.general.organization')}}</label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="popOrganizationId" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['organizationList']??[]) as $data)
                                                <option value="{{$data->organization_id}}">{{$data->organization_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-3 control-label">{{__('web_message.general.managerName')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="popManagerName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-3 control-label">{{__('web_message.general.role')}}</label>
                                    <div class="col-sm-4">
                                        <select class="form-select form-control" wire:model="modifyRoleId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['roleList']??[]) as $data)
                                                <option value="{{$data->role_id}}">{{$data->role_name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-3 control-label">{{__('web_message.general.createdAt')}}</label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popCreatedAt" disabled>
                                    </div>
                                </div>
                                @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                                    <div class="form-group">
                                        <label class="col-sm-3 control-label">
                                            {{__('web_message.general.reset')}} {{__('web_message.general.password')}}
                                        </label>
                                        <div class="col-sm-5">
                                            <input type="text" class="form-control" wire:model="modifyManagerPwd">
                                        </div>
                                    </div>
                                @endif
                                <div class="panel-group accordion">
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#settingGroup">{{__('web_message.settingGroup.title')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse in" id="settingGroup">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-3 control-label">
                                                        {{__('web_message.settingGroup.customized')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <select class="form-select form-control" wire:model="modifyCustomizedId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                                            @forelse ((session()->get('pageCacheData')['customizedList']??[]) as $data)
                                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-3 control-label">
                                                        {{__('web_message.settingGroup.cashFlow')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <select class="form-select form-control" wire:model="modifyCashFlowId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                                            @forelse ((session()->get('pageCacheData')['cashFlowList']??[]) as $data)
                                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-3 control-label">
                                                        {{__('web_message.settingGroup.smsGateway')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <select class="form-select form-control" wire:model="modifySmsGatewayId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                                            @forelse ((session()->get('pageCacheData')['smsGatewayList']??[]) as $data)
                                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-3 control-label">
                                                        {{__('web_message.settingGroup.miner')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <select class="form-select form-control" wire:model="modifyMinerId" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                                            @forelse ((session()->get('pageCacheData')['minerList']??[]) as $data)
                                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.status')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="modify-manager-status" class="toggle-switch" type="checkbox"
                                               wire:model="modifyManagerStatus" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                        <label for="modify-manager-status"></label>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                            wire:click="doModify">{{__('web_message.general.modify')}}</button>
                                </div>
                            </div>
                        </div>
                    @endif
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('input[name="datetimes"]').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('input[name="datetimes"]').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('input[name="datetimes"]').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('timeRange', {range: range});
                });
                $('#popClose').click(function () {
                    $('#dataPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
