terraform {
  source = "git::https://github.com/dfds/infrastructure-modules.git//compute/ecr-repo?ref=0.2.14"
}

include {
  path = "${find_in_parent_folders()}"
}

inputs = {
  names = [
    "harald/harald",
    "harald/dbmigrations",
  ]

  scan_on_push = true

  pull_principals= [
    "arn:aws:iam::738063116313:root",
  ]
}
