import 'package:collection/collection.dart';
import 'package:flutter/material.dart';

import '../models/top_up_record.dart';
import '../models/fund_record.dart';
import '../models/top_up_account.dart';
import '../models/top_up_bank.dart';
import '../models/wallet.dart';
import '../models/withdrawal_bank.dart';
import '../models/withdrawal_bank_account.dart';
import '../models/withdrawal_create.dart';
import '../models/withdrawal_record.dart';
import '../utils/http_service.dart';

class WalletProvider with ChangeNotifier {
  List<Wallet> _walletList = [];
  List<TopUpBank> _topUpBankList = [];
  TopUpAccount? _topUpAccount;
  TopUpRecord? _topUpRecord;
  String? _topUpUSDTPromo;
  List<WithdrawalBank> _withdrawalBankList = [];
  List<WithdrawalBank> _withdrawalCryptoList = [];
  WithdrawalAccount? _withdrawalAccount;
  WithdrawalCreate? _withdrawalCreate;
  WithdrawalRecord? _withdrawalRecord;
  FundRecord? _fundRecord;
  List<FundRecordType> _fundRecordTypeList = [];

  List<Wallet> get walletList => _walletList;
  Wallet? get mainWallet =>
      _walletList.firstWhereOrNull((wallet) => wallet.walletTypeId == "1");
  List<TopUpBank> get topUpBankList =>
      _topUpBankList.where((bank) => bank.bankType == "1").toList();
  List<TopUpBank> get topUpCryptoList =>
      _topUpBankList.where((bank) => bank.bankType == "3").toList();
  TopUpAccount? get topUpAccount => _topUpAccount;
  TopUpRecord? get topUpRecord => _topUpRecord;
  String? get topUpUSDTPromo => _topUpUSDTPromo;
  List<WithdrawalBank> get withdrawalBankList => [..._withdrawalBankList];
  List<WithdrawalBank> get withdrawalCryptoList => [..._withdrawalCryptoList];
  WithdrawalAccount? get withdrawalAccount => _withdrawalAccount;
  WithdrawalCreate? get withdrawalCreate => _withdrawalCreate;
  WithdrawalRecord? get withdrawalRecord => _withdrawalRecord;
  FundRecord? get fundRecord => _fundRecord;
  List<FundRecordType> get fundRecordTypeList => _fundRecordTypeList;

  void clear() {
    _walletList = [];
    _topUpBankList = [];
    _topUpAccount = null;
    _topUpRecord = null;
    _topUpUSDTPromo = null;
    _withdrawalBankList = [];
    _withdrawalCryptoList = [];
    _withdrawalAccount = null;
    _withdrawalCreate = null;
    _withdrawalRecord = null;
    _fundRecord = null;
    _fundRecordTypeList = [];
    notifyListeners();
  }

  Future<void> fetchBalance() async {
    final res = await HttpService.post(path: 'api/member/wallet');
    _walletList = List<Wallet>.from(
      res['data']['list'].map((item) => Wallet.fromJson(item)),
    );
    notifyListeners();
  }

  Future<void> fetchTopUpBank() async {
    final res = await HttpService.post(
      path: 'api/deposit/bank',
    );
    _topUpBankList = List<TopUpBank>.from(
      res['data']['list'].map((item) => TopUpBank.fromJson(item)),
    );
    _topUpUSDTPromo = res['data']['usdt_promo'];
    notifyListeners();
  }

  Future<void> fetchTopUpBankAccount({
    required String bankId,
    required String bankAccountId,
  }) async {
    final res = await HttpService.post(
      path: "api/deposit/account",
      data: {
        "bank_id": bankId,
        "bank_account_id": bankAccountId,
      },
    );
    _topUpAccount = TopUpAccount.fromJson(res['data']['account']);
    notifyListeners();
  }

  Future<Map<String, dynamic>?> topUpSubmit({
    required String bankId,
    required String amount,
    String? cardholder,
  }) async {
    final res = await HttpService.post(
      path: "api/deposit/create",
      data: {
        "bank_id": bankId,
        "amount": amount,
        if (cardholder != null) "cardholder": cardholder,
      },
    );
    return res['data'];
  }

  Future<void> topUpUploadReceipt({
    required String transactionNo,
    required String orderId,
    required String image,
  }) async {
    await HttpService.post(
      path: "api/deposit/updateImage",
      data: {
        "transaction_no": transactionNo,
        "order_id": orderId,
        "image": image,
      },
    );
  }

  Future<void> fetchTopUpRecord({
    required int days,
    required int page,
    required int perPage,
  }) async {
    final res = await HttpService.post(
      path: "api/deposit/orders",
      data: {"days": days, "page": page, "per_page": perPage},
    );
    _topUpRecord = TopUpRecord.fromJson(res['data']);
    notifyListeners();
  }

  Future<void> fetchWithdrawalBank() async {
    final res = await HttpService.post(path: "api/withdrawal/bank");
    _withdrawalBankList = List<WithdrawalBank>.from(
      res['data']['bank'].map((item) => WithdrawalBank.fromJson(item)),
    );
    _withdrawalCryptoList = List<WithdrawalBank>.from(
      res['data']['crypto'].map((item) => WithdrawalBank.fromJson(item)),
    );
    notifyListeners();
  }

  Future<void> fetchWithdrawalBankAccount() async {
    final res = await HttpService.post(path: "api/withdrawal/bankAccount");
    _withdrawalAccount = WithdrawalAccount.fromJson(res['data']);
    notifyListeners();
  }

  void setWithdrawalCreate(WithdrawalCreate? data) {
    _withdrawalCreate = data;
    notifyListeners();
  }

  Future<void> createWithdrawal() async {
    final data = _withdrawalCreate;
    if (data == null) return;
    await HttpService.post(
      path: "api/withdrawal/create",
      data: {
        "bank_id": data.bankId,
        "bank_tax": data.bankTax,
        "bank_tax_type": data.bankTaxType,
        "account_id": data.accountId,
        "amount": data.amount,
        "fund_password": data.fundPassword,
        "gauth_code": data.gauthCode,
        "rate": data.rate,
      },
    );
  }

  Future<void> fetchWithdrawalRecord({
    required int days,
    required int page,
    required int perPage,
  }) async {
    final res = await HttpService.post(
      path: "api/withdrawal/orders",
      data: {"days": days, "page": page, "per_page": perPage},
    );
    _withdrawalRecord = WithdrawalRecord.fromJson(res['data']);
    notifyListeners();
  }

  // 創建資金密碼
  Future<void> createFundPassword({
    required String fundPassword,
    required String confirmFundPassword,
  }) async {
    await HttpService.post(path: 'api/member/createFundPassword', data: {
      'fund_password': fundPassword,
      'fund_password_confirmation': confirmFundPassword,
    });
  }

  // 變更資金密碼
  Future<void> changeFundPassword({
    required String oldFundPassword,
    required String newFundPassword,
    required String confirmNewFundPassword,
  }) async {
    await HttpService.post(path: 'api/member/changeFundPassword', data: {
      'old_fund_password': oldFundPassword,
      'new_fund_password': newFundPassword,
      'new_fund_password_confirmation': confirmNewFundPassword,
    });
  }

  // 重設資金密碼
  Future<void> resetFundPassword({
    required String newFundPassword,
    required String confirmNewFundPassword,
    required String smsCode,
  }) async {
    await HttpService.post(path: 'api/member/resetFundPassword', data: {
      'new_fund_password': newFundPassword,
      'new_fund_password_confirmation': confirmNewFundPassword,
      'sms_code': smsCode,
    });
  }

  // 資金紀錄
  Future<void> fetchFundRecord({
    required int days,
    required int page,
    required int perPage,
    required String type,
  }) async {
    final res = await HttpService.post(path: 'api/record/fund', data: {
      'days': days,
      'type': type,
      'page': page,
      'per_page': perPage,
    });
    _fundRecord = FundRecord.fromJson(res['data']);
    notifyListeners();
  }

  // 資金紀錄類型
  Future<void> fetchFundRecordType() async {
    final res = await HttpService.post(path: 'api/dropdown/transType');
    _fundRecordTypeList = List<FundRecordType>.from(
      res['data']['list'].map((item) => FundRecordType.fromJson(item)),
    );
    notifyListeners();
  }
}
