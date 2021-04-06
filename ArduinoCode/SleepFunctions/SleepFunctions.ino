#include <avr/sleep.h>
#include <avr/power.h>

void setup() {
  // put your setup code here, to run once:

}

void loop() {
  // put your main code here, to run repeatedly:

}

void GoToSleep()
{
  set_sleep_mode(SLEEP_MODE_IDLE);
  sleep_enable();

  // Disable certain timers and other peripherals
  power_timer0_disable();
  power_timer2_disable();
  power_spi_disable();
  power_adc_disable();
  power_twi_disable();

  sleep_mode();
}

void ExitSleep()
{
  // Exit sleep and restore power to peripherals
  sleep_disable();
  power_all_enable();
}
