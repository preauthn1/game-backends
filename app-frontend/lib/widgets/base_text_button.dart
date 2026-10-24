import 'package:flutter/material.dart';

class BaseTextButton extends StatelessWidget {
  final Widget? child;
  final Function()? onPressed;
  const BaseTextButton({super.key, this.child, this.onPressed});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onPressed,
      borderRadius: BorderRadius.circular(4),
      highlightColor: Colors.grey.withOpacity(0.2),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 4),
        child: child,
      ),
    );
  }
}
