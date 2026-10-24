import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_gen/gen_l10n/app_localizations.dart';
import 'package:provider/provider.dart';
import './routes/app_routes.dart';
import './themes/theme.dart';
import 'providers/app_provider.dart';
import 'providers/auth_provider.dart';
import 'providers/member_provider.dart';
import 'providers/miner_provider.dart';
import 'providers/promotion_provider.dart';
import 'providers/wallet_provider.dart';

Future<void> main() async {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AppProvider()),
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => MemberProvider()),
        ChangeNotifierProvider(create: (_) => MinerProvider()),
        ChangeNotifierProvider(create: (_) => WalletProvider()),
        ChangeNotifierProvider(create: (_) => PromotionProvider()),
      ],
      child: MaterialApp.router(
        supportedLocales: const [
          Locale('en', ''),
          Locale('zh', ''),
        ],
        localizationsDelegates: const [
          GlobalMaterialLocalizations.delegate,
          GlobalWidgetsLocalizations.delegate,
          GlobalCupertinoLocalizations.delegate,
          AppLocalizations.delegate,
        ],
        debugShowCheckedModeBanner: false,
        title: 'Mybit',
        routerConfig: appRouter,
        themeMode: ThemeMode.dark,
        theme: AppTheme.lightTheme,
        darkTheme: AppTheme.darkTheme,
      ),
    );
  }
}
