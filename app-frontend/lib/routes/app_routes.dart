import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../screens/agent_screen.dart';
import '../screens/announcement_screen.dart';
import '../screens/miner_group_checkout_screen.dart';
import '../screens/miner_purchase_checkout_screen.dart';
import '../screens/referral_rewards_help_screen.dart';
import '../screens/sales_rebates_help_screen.dart';
import '../screens/splash_screen.dart';
import '../screens/trial_depreciation_screen.dart';
import '../screens/faq_detail_screen.dart';
import '../screens/faq_screen.dart';
import '../screens/fund_record_screen.dart';
import '../screens/miner_group_list_screen.dart';
import '../screens/miners_group_screen.dart';
import '../screens/miner_purchase_detail_screen.dart';
import '../screens/miner_rent_detail_screen.dart';
import '../screens/miner_rent_list_screen.dart';
import '../screens/miners_screen.dart';
import '../screens/my_miner_balance_screen.dart';
import '../screens/my_miner_detail_screen.dart';
import '../screens/navigation_root.dart';
import '../screens/login_screen.dart';
import '../screens/profile/bank_card_screen.dart';
import '../screens/profile/change_fund_password_screen.dart';
import '../screens/profile/crypto_address_screen.dart';
import '../screens/profile/create_fund_password_screen.dart';
import '../screens/profile/google_authentication_screen.dart';
import '../screens/profile/id_information_screen.dart';
import '../screens/profile/login_password_screen.dart';
import '../screens/profile/profile_screen.dart';
import '../screens/profile/avatar_screen.dart';
import '../screens/profile/reset_fund_password_screen.dart';
import '../screens/profile/username_screen.dart';
import '../screens/profile/email_screen.dart';
import '../screens/profile/mobile_screen.dart';
import '../screens/promotion_screen.dart';
import '../screens/referral_reward_screen.dart';
import '../screens/register_screen.dart';
import '../screens/reset_password_screen.dart';
import '../screens/sales_rebate_screen.dart';
import '../screens/top_up_receipt_screen.dart';
import '../screens/top_up_record_screen.dart';
import '../screens/top_up_screen.dart';
import '../screens/withdrawal_record_screen.dart';
import '../screens/withdrawal_screen.dart';
import '../screens/withdrawal_verify_screen.dart';

final GoRouter appRouter = GoRouter(
  initialLocation: '/',
  redirect: (context, state) {
    final location = state.uri.toString();
    if (location == '/') {
      return null;
    }
    final isLoggedIn = context.read<AuthProvider>().token != null;
    final unAuthRoutes = [
      '/login',
      '/register',
      '/reset-password',
    ];
    if (isLoggedIn && unAuthRoutes.contains(location)) {
      return '/';
    } else if (!isLoggedIn && !unAuthRoutes.contains(location)) {
      return '/';
    }
    return null;
  },
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) => const SplashScreen(),
    ),
    GoRoute(
      path: '/login',
      builder: (context, state) => const LoginScreen(),
    ),
    GoRoute(
      path: '/register',
      builder: (context, state) => const RegisterScreen(),
    ),
    GoRoute(
      path: '/reset-password',
      builder: (context, state) => const ResetPasswordScreen(),
    ),
    GoRoute(
      path: '/home',
      builder: (context, state) => const NavigationRoot(),
    ),
    GoRoute(
      path: '/miners',
      builder: (context, state) => const MinersScreen(),
    ),
    GoRoute(
      path: '/miners/purchase/:minerId/detail',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        return MinerPurchaseDetailScreen(minerId: id!);
      },
    ),
    GoRoute(
      path: '/miners/purchase/:minerId/checkout',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        return MinerPurchaseCheckoutScreen(id: id!);
      },
    ),
    GoRoute(
      path: '/miners/rent/:minerId/list',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        final idToInt = int.tryParse(id!) ?? -1;
        return MinerRentListScreen(minerId: idToInt);
      },
    ),
    GoRoute(
      path: '/miners/rent/:minerId/:mmId',
      builder: (context, state) {
        final id = state.pathParameters['mmId'];
        return MinerRentDetailScreen(mmId: id!);
      },
    ),
    GoRoute(
      path: '/miners/group',
      builder: (context, state) => const MinersGroupScreen(),
    ),
    GoRoute(
      path: '/miners/group/:minerId/detail',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        return MinerPurchaseDetailScreen(minerId: id!, isGroup: true);
      },
    ),
    GoRoute(
      path: '/miners/group/:minerId/list',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        final idToInt = int.tryParse(id!) ?? -1;
        return MinerGroupListScreen(minerId: idToInt);
      },
    ),
    GoRoute(
      path: '/miners/group/:minerId/create',
      builder: (context, state) {
        final minerId = state.pathParameters['minerId'];
        return MinerGroupCheckoutScreen(minerId: minerId!);
      },
    ),
    GoRoute(
      path: '/miners/group/:minerId/:mmId/checkout',
      builder: (context, state) {
        final minerId = state.pathParameters['minerId'];
        final mmId = state.pathParameters['mmId'];
        return MinerGroupCheckoutScreen(minerId: minerId!, mmId: mmId!);
      },
    ),
    GoRoute(
      path: '/my-miner/purchase/:minerId/detail',
      builder: (context, state) {
        final id = state.pathParameters['minerId'];
        final idToInt = int.tryParse(id!) ?? -1;
        return MyMinerDetailScreen(id: idToInt);
      },
    ),
    GoRoute(
      path: '/my-miner/purchase/:minerId/:mmId/balance',
      builder: (context, state) {
        final id = state.pathParameters['mmId'];
        return MyMinerBalanceScreen(mmId: id!);
      },
    ),
    GoRoute(
      path: '/announcement',
      builder: (context, state) => const AnnouncementScreen(),
    ),
    GoRoute(
      path: '/promotion',
      builder: (context, state) => const PromotionScreen(),
    ),
    GoRoute(
      path: '/agent',
      builder: (context, state) => const AgentScreen(),
    ),
    GoRoute(
      path: '/record/fund',
      builder: (context, state) => const FundRecordScreen(),
    ),
    GoRoute(
      path: '/record/top-up',
      builder: (context, state) => const TopUpRecordScreen(),
    ),
    GoRoute(
      path: '/record/withdrawal',
      builder: (context, state) => const WithdrawalRecordScreen(),
    ),
    GoRoute(
      path: '/trial-depreciation',
      builder: (context, state) => const TrialDepreciationScreen(),
    ),
    GoRoute(
      path: '/sales-rebate',
      builder: (context, state) => const SalesRebateScreen(),
    ),
    GoRoute(
      path: '/referral-reward',
      builder: (context, state) => const ReferralRewardScreen(),
    ),
    GoRoute(
      path: '/faq',
      builder: (context, state) => const FAQScreen(),
    ),
    GoRoute(
      path: '/faq-detail/:faqId',
      builder: (context, state) {
        final faqId = state.pathParameters['faqId'];
        return FAQDetailScreen(faqId: faqId!);
      },
    ),
    GoRoute(
      path: '/sales-rebates-help',
      builder: (context, state) => const SalesRebatesHelpScreen(),
    ),
    GoRoute(
      path: '/referral-rewards-help',
      builder: (context, state) => const ReferralRewardsHelpScreen(),
    ),
    GoRoute(
      path: '/top-up',
      builder: (context, state) => const TopUpScreen(),
    ),
    GoRoute(
      path: '/top-up/receipt',
      builder: (context, state) {
        final query = state.uri.queryParameters;
        final bankId = query['bankId'];
        final orderId = query['orderId'];
        final bankAccountId = query['bankAccountId'];
        return TopUpReceiptScreen(
          bankId: bankId,
          orderId: orderId,
          bankAccountId: bankAccountId,
        );
      },
    ),
    GoRoute(
      path: '/withdrawal',
      builder: (context, state) => const WithdrawalScreen(),
    ),
    GoRoute(
      path: '/withdrawal/verify',
      builder: (context, state) => const WithdrawalVerifyScreen(),
    ),
    GoRoute(
      path: '/profile',
      builder: (context, state) => const ProfileScreen(),
      routes: [
        GoRoute(
          path: 'avatar',
          builder: (context, state) => const AvatarScreen(),
        ),
        GoRoute(
          path: 'username',
          builder: (context, state) => const UsernameScreen(),
        ),
        GoRoute(
          path: 'email',
          builder: (context, state) => const EmailScreen(),
        ),
        GoRoute(
          path: 'mobile',
          builder: (context, state) => const MobileScreen(),
        ),
        GoRoute(
          path: 'login-password',
          builder: (context, state) => const LoginPasswordScreen(),
        ),
        GoRoute(
          path: 'create-fund-password',
          builder: (context, state) => const CreateFundPasswordScreen(),
        ),
        GoRoute(
          path: 'change-fund-password',
          builder: (context, state) => const ChangeFundPasswordScreen(),
        ),
        GoRoute(
          path: 'reset-fund-password',
          builder: (context, state) => const ResetFundPasswordScreen(),
        ),
        GoRoute(
          path: 'crypto-address',
          builder: (context, state) => const CryptoAddressScreen(),
        ),
        GoRoute(
          path: 'bank-card',
          builder: (context, state) => const BankCardScreen(),
        ),
        GoRoute(
          path: 'id-information',
          builder: (context, state) => const IdInformationScreen(),
        ),
        GoRoute(
          path: 'google-authentication',
          builder: (context, state) => const GoogleAuthenticationScreen(),
        ),
      ],
    ),
  ],
);
