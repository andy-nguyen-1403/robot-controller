pipeline {
    agent any

    environment {
        PATH = "/usr/local/share/dotnet:/usr/local/bin:/opt/homebrew/bin:/opt/homebrew/sbin:${env.PATH}"

        IMAGE_NAME = "robot-api"
        IMAGE_TAG = "build-${BUILD_NUMBER}"
        ROBOT_IMAGE = "robot-api:build-${BUILD_NUMBER}"

        STAGING_PORT = "8080"
        PRODUCTION_PORT = "8081"
    }

    stages {

        // ============================================================
        // 1. BUILD
        // ============================================================
        stage('1. Build') {
            steps {
                echo "===== BUILD STAGE ====="

                sh '''
                    set -e

                    echo "Building RobotController..."
                    dotnet restore RobotController/RobotController.csproj

                    dotnet build RobotController/RobotController.csproj \
                        --configuration Release \
                        --no-restore

                    echo "Building Robot API..."
                    dotnet restore robot-api/robot-api.csproj

                    dotnet build robot-api/robot-api.csproj \
                        --configuration Release \
                        --no-restore

                    echo "Building Docker image..."
                    echo "Image: ${ROBOT_IMAGE}"

                    docker build \
                        -t "${ROBOT_IMAGE}" \
                        robot-api

                    echo "===== BUILD COMPLETE ====="
                    docker images "${IMAGE_NAME}"
                '''
            }
        }


        // ============================================================
        // 2. TEST
        // ============================================================
        stage('2. Test') {
            steps {
                echo "===== TEST STAGE ====="

                sh '''
                    set -e

                    echo "===== CLEANING TEST OUTPUT ====="

                    rm -rf TestResults
                    rm -rf RobotTests/TestResults

                    mkdir -p TestResults

                    echo "===== RUNNING TESTS ====="

                    dotnet test RobotTests/RobotTests.csproj \
                        --configuration Release \
                        --no-restore \
                        --logger "trx;LogFileName=test-results.trx" \
                        --collect:"XPlat Code Coverage" \
                        --settings coverlet.runsettings \
                        --results-directory "$WORKSPACE/TestResults"

                    TEST_EXIT=$?

                    echo "dotnet test exit code: $TEST_EXIT"

                    if [ "$TEST_EXIT" -ne 0 ]; then
                        echo "ERROR: Automated tests failed."
                        exit "$TEST_EXIT"
                    fi

                    echo "===== SEARCHING FOR TEST OUTPUT ====="

                    echo "--- Workspace TestResults ---"
                    find "$WORKSPACE/TestResults" \
                        -type f \
                        -print || true

                    echo "--- RobotTests/TestResults ---"
                    find "$WORKSPACE/RobotTests/TestResults" \
                        -type f \
                        -print 2>/dev/null || true

                    echo "===== LOCATING TRX REPORT ====="

                    TEST_REPORT=$(find "$WORKSPACE" \
                        -type f \
                        -name "*.trx" \
                        -print -quit)

                    if [ -z "$TEST_REPORT" ]; then
                        echo "ERROR: No TRX test report was generated."
                        exit 1
                    fi

                    echo "Test report found:"
                    echo "$TEST_REPORT"

                    echo "===== LOCATING OPENCOVER REPORT ====="

                    COVERAGE_FILE=$(find "$WORKSPACE" \
                        -type f \
                        -name "coverage.opencover.xml" \
                        -print -quit)

                    if [ -z "$COVERAGE_FILE" ]; then
                        echo "ERROR: No OpenCover coverage report was generated."
                        exit 1
                    fi

                    echo "Coverage report found:"
                    echo "$COVERAGE_FILE"

                    echo "===== TEST STAGE PASSED ====="
                '''
            }

            post {
                always {
                    junit allowEmptyResults: true,
                        testResults: '**/*.trx'

                    archiveArtifacts(
                        artifacts: '**/coverage.opencover.xml,**/*.trx',
                        allowEmptyArchive: true
                    )
                }
            }
        }


        // ============================================================
        // 3. CODE QUALITY
        // ============================================================
        stage('3. Code Quality') {
            steps {
                echo "===== CODE QUALITY STAGE ====="

                withSonarQubeEnv('SonarQube') {

                    sh '''
                        set -e

                        echo "Starting SonarQube analysis..."

                        rm -rf .sonarqube

                        dotnet sonarscanner begin \
                            /k:"robot-controller" \
                            /d:sonar.host.url="$SONAR_HOST_URL" \
                            /d:sonar.token="$SONAR_AUTH_TOKEN" \
                            /d:sonar.cs.opencover.reportsPaths="TestResults/**/coverage.opencover.xml"

                        echo "Building RobotController for SonarQube..."

                        dotnet build RobotController/RobotController.csproj \
                            --configuration Release \
                            --no-restore

                        echo "Building Robot API for SonarQube..."

                        dotnet build robot-api/robot-api.csproj \
                            --configuration Release \
                            --no-restore

                        echo "Finishing SonarQube analysis..."

                        dotnet sonarscanner end \
                            /d:sonar.token="$SONAR_AUTH_TOKEN"

                        echo "===== SONARQUBE ANALYSIS COMPLETE ====="
                    '''
                }
            }
        }


        // ============================================================
        // 4. SECURITY
        // ============================================================
        stage('4. Security') {
            steps {
                echo "===== SECURITY STAGE ====="

                sh '''
                    set -e

                    mkdir -p security-reports

                    echo "Scanning Docker image with Trivy..."
                    echo "Image: ${ROBOT_IMAGE}"

                    trivy image \
                        --severity HIGH,CRITICAL \
                        --ignore-unfixed \
                        --exit-code 1 \
                        "${ROBOT_IMAGE}"

                    echo "Generating Trivy JSON report..."

                    trivy image \
                        --severity HIGH,CRITICAL \
                        --ignore-unfixed \
                        --format json \
                        --output security-reports/trivy-report.json \
                        "${ROBOT_IMAGE}"

                    echo "===== SECURITY SCAN PASSED ====="
                    echo "No HIGH or CRITICAL unfixed vulnerabilities found."
                '''
            }

            post {
                always {
                    archiveArtifacts(
                        artifacts: 'security-reports/trivy-report.json',
                        allowEmptyArchive: true
                    )
                }
            }
        }


        // ============================================================
        // 5. DEPLOY - STAGING
        // ============================================================
        stage('5. Deploy') {
            steps {
                echo "===== DEPLOY STAGE ====="

                sh '''
                    set -e

                    echo "Deploying image to STAGING..."
                    echo "Image: ${ROBOT_IMAGE}"

                    docker compose \
                        -p robot-staging \
                        -f robot-api/docker-compose.yml \
                        down || true

                    ROBOT_IMAGE="${ROBOT_IMAGE}" \
                    docker compose \
                        -p robot-staging \
                        -f robot-api/docker-compose.yml \
                        up -d

                    echo "Waiting for staging application..."
                    sleep 5

                    echo "===== STAGING CONTAINER ====="

                    docker compose \
                        -p robot-staging \
                        -f robot-api/docker-compose.yml \
                        ps

                    echo "===== STAGING HEALTH CHECK ====="

                    curl --fail --silent \
                        http://localhost:${STAGING_PORT}/health

                    echo

                    echo "===== STAGING ROOT CHECK ====="

                    curl --fail --silent \
                        http://localhost:${STAGING_PORT}/

                    echo

                    echo "===== STAGING ROBOT COMMAND CHECK ====="

                    curl --fail --silent \
                        http://localhost:${STAGING_PORT}/robot-commands

                    echo

                    echo "===== STAGING DEPLOYMENT PASSED ====="
                '''
            }
        }


        // ============================================================
        // 6. RELEASE - PRODUCTION
        // ============================================================
        stage('6. Release') {
            steps {
                echo "===== RELEASE STAGE ====="

                sh '''
                    set -e

                    echo "Releasing tested image to PRODUCTION..."
                    echo "Image: ${ROBOT_IMAGE}"

                    docker compose \
                        -p robot-production \
                        -f robot-api/docker-compose.production.yml \
                        down || true

                    ROBOT_IMAGE="${ROBOT_IMAGE}" \
                    docker compose \
                        -p robot-production \
                        -f robot-api/docker-compose.production.yml \
                        up -d

                    echo "Waiting for production application..."
                    sleep 5

                    echo "===== PRODUCTION CONTAINER ====="

                    docker compose \
                        -p robot-production \
                        -f robot-api/docker-compose.production.yml \
                        ps

                    echo "===== PRODUCTION HEALTH CHECK ====="

                    curl --fail --silent \
                        http://localhost:${PRODUCTION_PORT}/health

                    echo

                    echo "===== PRODUCTION ROOT CHECK ====="

                    curl --fail --silent \
                        http://localhost:${PRODUCTION_PORT}/

                    echo

                    echo "===== PRODUCTION COMMAND CHECK ====="

                    curl --fail --silent \
                        http://localhost:${PRODUCTION_PORT}/robot-commands

                    echo

                    echo "===== PRODUCTION RELEASE PASSED ====="
                    echo "Released image: ${ROBOT_IMAGE}"
                '''
            }
        }


        // ============================================================
        // 7. MONITORING
        // ============================================================
        stage('7. Monitoring') {
            steps {
                echo "===== MONITORING STAGE ====="

                sh '''
                    set -e

                    echo "Checking production health..."

                    curl --fail --silent \
                        http://localhost:${PRODUCTION_PORT}/health

                    echo

                    echo "Checking production container status..."

                    docker inspect \
                        --format='Container={{.Name}} Status={{.State.Status}} Health={{if .State.Health}}{{.State.Health.Status}}{{else}}not-configured{{end}}' \
                        robot-api-production

                    echo "Checking production container is running..."

                    STATUS=$(docker inspect \
                        --format='{{.State.Status}}' \
                        robot-api-production)

                    if [ "$STATUS" != "running" ]; then
                        echo "ALERT: robot-api-production is not running."
                        exit 1
                    fi

                    echo "===== MONITORING PASSED ====="
                    echo "Production service is healthy and running."
                '''
            }
        }
    }


    // ================================================================
    // PIPELINE RESULT
    // ================================================================
    post {

        success {
            echo """
            ==========================================
                    PIPELINE SUCCESS
            ==========================================
            Build: ${BUILD_NUMBER}
            Image: ${ROBOT_IMAGE}

            All 7 pipeline stages completed:
            1. Build
            2. Test
            3. Code Quality
            4. Security
            5. Deploy
            6. Release
            7. Monitoring
            ==========================================
            """
        }

        failure {
            echo """
            ==========================================
                    PIPELINE FAILED
            ==========================================
            Build: ${BUILD_NUMBER}

            Check the failed stage in the console log.
            ==========================================
            """
        }

        always {
            echo "Pipeline execution finished."
        }
    }
}
