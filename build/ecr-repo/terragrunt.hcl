terraform {
  source = "git::https://github.com/dfds/infrastructure-modules.git//compute/ecr-repo"
}

include {
  path = "${find_in_parent_folders()}"
}

inputs = {
  list_of_repos = [
    "harald/harald",
    "harald/dbmigrations",
  ]

  scan_images = true

  accounts = [
    "arn:aws:iam::738063116313:root",
  ]
}
